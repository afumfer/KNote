using System.Diagnostics;
using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.Model;
using KntIcons;
using Microsoft.Web.WebView2.Core;

namespace KNote.ClientWin.Views;

// Preview window of a printable report. The report's HTML is served from memory under ReportUrl (not via
// NavigateToString, which is limited to 2 MB - a long notes list can exceed it), with scripts disabled:
// part of the content (note descriptions) is user written. Printing uses the browser's own print dialog,
// which has its own paged preview, printer selection and "Save as PDF"; "Save as PDF ..." here saves
// directly in the report's orientation.
// A report with page references (e.g. the contents of a book, ReportDocument.ResolvePageReferences) is
// paginated in two passes before it is shown: printed once to an in-memory PDF to read where each target
// landed (PdfNamedDestinations), then reloaded with those page numbers filled in.
// webView.DefaultBackgroundColor must stay white (set in the Designer): the print engine paints the page
// margins with it, not with the document's background - grey there gave PDFs grey borders. The grey around
// the sheet in the preview comes from the report's own stylesheet (@media screen).
public partial class ReportPreviewForm : KntForm, IViewBase
{
    #region Private fields

    // Served from memory; each load gets its own "?v=N" so a reload after filling in the page numbers is
    // a real navigation.
    private const string ReportHost = "https://knote.report/";
    private const string ReportUrl = ReportHost + "report.html";

    // A4 in inches (WebView2 print settings unit), portrait; Orientation rotates it.
    private const double A4WidthInches = 8.27;
    private const double A4HeightInches = 11.69;

    private static readonly double[] ZoomLevels = { 0.5, 0.67, 0.75, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2 };

    private readonly ReportPreviewCtrl _ctrl;
    private bool _webViewReady;
    private int _loadCount;

    #endregion

    #region Constructor

    public ReportPreviewForm(ReportPreviewCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        buttonPrint.SetKntIcon(KntIcon.Print);
        buttonSavePdf.SetKntIcon(KntIcon.SavePdf);
        buttonZoomOut.SetKntIcon(KntIcon.ZoomOut);
        buttonZoomIn.SetKntIcon(KntIcon.ZoomIn);
        buttonClose.SetKntIcon(KntIcon.Close);

        SizeZoomLabel();
        EnableReportActions(false);
    }

    // labelZoom has a fixed size (so the toolbar doesn't jump as the zoom changes), but the designer's
    // size isn't scaled with DPI and clips the text at 200%: size it from the widest text it can show.
    private void SizeZoomLabel()
    {
        var textSize = TextRenderer.MeasureText("000 %", labelZoom.Font);
        labelZoom.Size = new Size(textSize.Width + toolBarReport.LogicalToDeviceUnits(8),
            textSize.Height + toolBarReport.LogicalToDeviceUnits(4));
    }

    #endregion

    #region IViewBase interface

    public override void ShowView()
    {
        Text = $"{KntConst.AppName} - {_ctrl.Report?.Title}";
        statusLabel.Text = _ctrl.Report?.Orientation == ReportOrientation.Landscape ? "A4 landscape" : "A4 portrait";
        SetInitialSize();
        base.ShowView();
    }

    #endregion

    #region Form events handlers

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

    private async void ReportPreviewForm_Shown(object sender, EventArgs e)
    {
        try
        {
            await InitializeWebViewAsync();

            if (_ctrl.Report?.ResolvePageReferences == true)
                await ResolvePageReferencesAsync();

            await LoadReportAsync();
            _webViewReady = true;
            EnableReportActions(true);
        }
        catch (Exception ex)
        {
            await LeaveWebView2CallbackAsync();
            _ctrl.ReportError("The report preview could not be initialized", ex);
        }
    }

    private async void buttonToolBar_Click(object sender, EventArgs e)
    {
        if (sender == buttonClose)
        {
            Close();
            return;
        }

        if (!_webViewReady)
            return;

        if (sender == buttonPrint)
            Print();
        else if (sender == buttonSavePdf)
            await SavePdfAsync();
        else if (sender == buttonZoomIn)
            ChangeZoom(+1);
        else if (sender == buttonZoomOut)
            ChangeZoom(-1);
    }

    private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        if (!IsReportUri(e.Request.Uri))
            return;

        var content = new MemoryStream(Encoding.UTF8.GetBytes(_ctrl.Html ?? ""));
        e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(content, 200, "OK", "Content-Type: text/html; charset=utf-8");
    }

    // The report is a static page: a click on a link of a note description opens it in the default browser
    // instead of navigating the preview away from the report.
    private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (IsReportUri(e.Uri))
            return;

        e.Cancel = true;
        OpenLink(e.Uri);
    }

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        OpenLink(e.Uri);
    }

    #endregion

    #region Private methods

    private async Task InitializeWebViewAsync()
    {
        // Same user data folder (and so the same browser process) as the note editors' WebView2 instances.
        var userDataFolder = KntWebView.KntEditView.WebView2UserDataFolder ?? Path.Combine(Application.StartupPath, "WebView2Cache");
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await webView.EnsureCoreWebView2Async(environment);

        var core = webView.CoreWebView2;
        core.Settings.IsScriptEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = true;
#if !DEBUG
        core.Settings.AreDevToolsEnabled = false;
#endif

        core.AddWebResourceRequestedFilter(ReportHost + "*", CoreWebView2WebResourceContext.Document);
        core.WebResourceRequested += CoreWebView2_WebResourceRequested;
        core.NavigationStarting += CoreWebView2_NavigationStarting;
        core.NewWindowRequested += CoreWebView2_NewWindowRequested;
        webView.ZoomFactorChanged += (s, e) => UpdateZoomLabel();

        if (!string.IsNullOrEmpty(_ctrl.ResourcesRootPath) && Directory.Exists(_ctrl.ResourcesRootPath))
            core.SetVirtualHostNameToFolderMapping(new Uri(KntConst.VirtualHostNameToFolderMapping).Host,
                _ctrl.ResourcesRootPath, CoreWebView2HostResourceAccessKind.Allow);
    }

    // Same URL plus "?v=N" (fragments "#..." of the contents' links included).
    private static bool IsReportUri(string uri)
        => uri != null && uri.StartsWith(ReportUrl, StringComparison.OrdinalIgnoreCase);

    private Task LoadReportAsync()
    {
        var loaded = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webView.CoreWebView2.NavigationCompleted -= OnCompleted;
            loaded.TrySetResult(e.IsSuccess);
        }

        webView.CoreWebView2.NavigationCompleted += OnCompleted;
        webView.CoreWebView2.Navigate($"{ReportUrl}?v={++_loadCount}");
        return loaded.Task;
    }

    private async Task ResolvePageReferencesAsync()
    {
        var status = statusLabel.Text;
        statusLabel.Text = "Paginating ...";
        UseWaitCursor = true;
        try
        {
            await LoadReportAsync();
            using var pdf = await webView.CoreWebView2.PrintToPdfStreamAsync(CreatePrintSettings());
            await LeaveWebView2CallbackAsync();
            using var bytes = new MemoryStream();
            await pdf.CopyToAsync(bytes);
            _ctrl.ApplyPageNumbers(PdfNamedDestinations.Parse(bytes.ToArray()));
        }
        finally
        {
            UseWaitCursor = false;
            statusLabel.Text = status;
        }
    }

    // The same settings for the pagination pass and "Save as PDF", so both paginate alike.
    private CoreWebView2PrintSettings CreatePrintSettings()
    {
        var settings = webView.CoreWebView2.Environment.CreatePrintSettings();
        settings.Orientation = _ctrl.Report.Orientation == ReportOrientation.Landscape
            ? CoreWebView2PrintOrientation.Landscape
            : CoreWebView2PrintOrientation.Portrait;
        settings.PageWidth = A4WidthInches;
        settings.PageHeight = A4HeightInches;
        settings.ShouldPrintBackgrounds = true;
        settings.ShouldPrintHeaderAndFooter = false;
        return settings;
    }

    private void Print()
    {
        try
        {
            webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
        }
        catch (Exception ex)
        {
            _ctrl.ReportError("The report could not be printed", ex);
        }
    }

    private async Task SavePdfAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Save report as PDF",
            Filter = "PDF document (*.pdf)|*.pdf",
            DefaultExt = "pdf",
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = _ctrl.LastExportFolder,
            FileName = _ctrl.SuggestedPdfFileName
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var saved = false;
        Exception error = null;
        try
        {
            using (new WaitCursor())
                saved = await webView.CoreWebView2.PrintToPdfAsync(dialog.FileName, CreatePrintSettings());
        }
        catch (Exception ex)
        {
            error = ex;
        }

        await LeaveWebView2CallbackAsync();

        if (error != null)
        {
            _ctrl.ReportError("The PDF file could not be saved", error);
            return;
        }

        if (!saved)
        {
            ShowInfo($"The PDF file could not be saved:\r\n{dialog.FileName}", "KNote - Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _ctrl.PdfSaved(dialog.FileName);
        if (ShowInfo($"Report saved as PDF:\r\n{dialog.FileName}\r\n\r\nDo you want to open it now?", "KNote - Report",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            OpenExternal(dialog.FileName);
    }

    // An awaited WebView2 call (PrintToPdfAsync, EnsureCoreWebView2Async...) resumes INSIDE WebView2's own
    // completion callback: the continuation runs inline on the UI thread. A modal dialog shown there runs
    // a nested message loop within that callback, which WebView2 does not support - clicking the dialog
    // killed the process (fail-fast in EmbeddedBrowserWebView.dll, no .NET exception). Task.Yield posts the
    // rest of the caller to the message queue (WinForms SynchronizationContext), so it runs once the
    // callback has returned. Call it before any modal UI that follows an awaited WebView2 call.
    private static async Task LeaveWebView2CallbackAsync()
        => await Task.Yield();

    private void ChangeZoom(int direction)
    {
        var current = webView.ZoomFactor;
        var next = direction > 0
            ? ZoomLevels.FirstOrDefault(z => z > current + 0.001, ZoomLevels[^1])
            : ZoomLevels.LastOrDefault(z => z < current - 0.001, ZoomLevels[0]);
        webView.ZoomFactor = next;
        UpdateZoomLabel();
    }

    private void UpdateZoomLabel()
    {
        labelZoom.Text = $"{Math.Round(webView.ZoomFactor * 100)} %";
    }

    private void EnableReportActions(bool enabled)
    {
        buttonPrint.Enabled = enabled;
        buttonSavePdf.Enabled = enabled;
        buttonZoomIn.Enabled = enabled;
        buttonZoomOut.Enabled = enabled;
    }

    // Wide enough to show a whole A4 sheet (landscape or portrait) at 100 %, never larger than the screen.
    private void SetInitialSize()
    {
        var landscape = _ctrl.Report?.Orientation == ReportOrientation.Landscape;
        var wanted = LogicalToDeviceUnits(new Size(landscape ? 1220 : 900, 860));
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        Size = new Size(Math.Min(wanted.Width, area.Width), Math.Min(wanted.Height, (int)(area.Height * 0.92)));
    }

    // Links come from user written content: only web and mail links are handed to the shell.
    private void OpenLink(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeMailto))
            OpenExternal(parsed.AbsoluteUri);
    }

    private void OpenExternal(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _ctrl.ReportError($"Could not open {target}", ex);
        }
    }

    #endregion
}
