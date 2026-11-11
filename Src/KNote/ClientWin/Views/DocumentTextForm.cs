using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// KNote Atlas, stage 4 (4.4b): the text KNote read of a document of the note, whole (decided by the owner: the Web shows its
// beginning), with its pages and the scanned ones marked, put in the box in pieces with a progress bar (a document can have
// thousands of pages). «Read document» / «Read again» reads it now (Staff: the command checks it), asking first if it was read.
// 4.5b: «Transcribe scanned pages» (NoteEditorCtrl.TranscribePagesAsync; the button cancels while it runs, and closing the
// window too), and «See the original page» with the page where the cursor is (DocumentPageForm, the PDF at that page).
public partial class DocumentTextForm : KntForm
{
    // What is put in the box at a time, so that the window keeps answering and the bar moves.
    private const int PieceLength = 256 * 1024;

    private readonly NoteEditorCtrl _ctrl;
    private readonly ResourceDto _resource;
    private readonly bool _paged;
    private readonly bool _isPdf;
    private readonly bool _transcribeWhenShown;
    private EnumExtractedState _state;
    // The text in the box (the page where the cursor is is looked for in it, without copying the box's text).
    private string _readable = "";
    private CancellationTokenSource _loading;
    private CancellationTokenSource _transcribing;

    public DocumentTextForm()
    {
        InitializeComponent();
    }

    // transcribe: «Transcribe the scanned pages of the selected document» of the note's editor, which starts it when shown.
    public DocumentTextForm(NoteEditorCtrl ctrl, ResourceDto resource, bool transcribe = false) : this()
    {
        _ctrl = ctrl;
        _resource = resource;
        var format = DocumentTextRules.FormatOf(resource.FileType, resource.Name);
        _paged = DocumentTextRules.IsPaged(format);
        _isPdf = format == EnumDocumentFormat.Pdf;
        _transcribeWhenShown = transcribe;
        Text = $"{resource.GetTitle()} - text";
        labelFile.Text = resource.NameOut;
        buttonTranscribe.Text = DocumentTexts.TranscribePages;
        buttonOriginalPage.Text = DocumentTexts.SeeOriginalPage;
        labelPage.Visible = numericPage.Visible = buttonOriginalPage.Visible = _isPdf;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadTextAsync();
        if (_transcribeWhenShown && !IsDisposed)
            await TranscribeAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _loading?.Cancel();
        _transcribing?.Cancel();
        base.OnFormClosing(e);
    }

    private async Task LoadTextAsync()
    {
        _loading?.Cancel();
        var loading = _loading = new CancellationTokenSource();

        textDocument.Clear();
        _readable = "";
        buttonRead.Enabled = false;
        buttonTranscribe.Visible = false;
        progressLoad.Value = 0;
        progressLoad.Visible = true;
        labelState.Text = "Loading the text…";

        var result = await _ctrl.GetDocumentTextAsync(_resource);
        if (loading.IsCancellationRequested || IsDisposed)
            return;
        if (!result.IsValid || result.Entity == null)
        {
            progressLoad.Visible = false;
            labelState.Text = result.ErrorMessage;
            return;
        }

        _state = result.Entity.State;
        var pages = DocumentTextRules.Pages(result.Entity.Text);
        var text = await Task.Run(() => DocumentTexts.Readable(result.Entity.Text, _paged));
        if (loading.IsCancellationRequested || IsDisposed)
            return;

        for (var start = 0; start < text.Length; start += PieceLength)
        {
            textDocument.AppendText(text.Substring(start, Math.Min(PieceLength, text.Length - start)));
            progressLoad.Value = (int)(100L * Math.Min(text.Length, start + PieceLength) / text.Length);
            await Task.Delay(1);
            if (loading.IsCancellationRequested || IsDisposed)
                return;
        }
        textDocument.SelectionStart = 0;
        textDocument.ScrollToCaret();
        _readable = text;

        progressLoad.Visible = false;
        var state = DocumentTexts.State(_state);
        if (_state == EnumExtractedState.Partial)
            state += " " + DocumentTexts.ScannedPageHelp;
        else if (_state != EnumExtractedState.NotRead && text.Length == 0)
            state += " " + DocumentTexts.NothingRead;
        if (pages.Any(p => p.Transcribed))
            state += " " + DocumentTexts.AutomaticTranscriptionHelp;
        labelState.Text = state;
        buttonRead.Text = _state == EnumExtractedState.NotRead ? DocumentTexts.ReadDocument : DocumentTexts.ReadAgain;
        buttonRead.Enabled = true;
        buttonTranscribe.Visible = _isPdf && _state == EnumExtractedState.Partial;
        numericPage.Maximum = Math.Max(1, pages.Count);
        numericPage.Value = 1;
    }

    private async void buttonRead_Click(object sender, EventArgs e)
    {
        if (_state != EnumExtractedState.NotRead
            && ShowInfo(DocumentTexts.ReadAgainQuestion, KntConst.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        buttonRead.Enabled = false;
        labelState.Text = "Reading the document…";
        UseWaitCursor = true;
        Result<DocumentReadDto> result;
        try
        {
            result = await _ctrl.ReadDocumentAsync(_resource);
        }
        finally
        {
            UseWaitCursor = false;
        }
        if (IsDisposed)
            return;

        if (result.IsValid)
            ShowInfo(DocumentTexts.ReadResult(result.Entity), DocumentTexts.ReadDocument, MessageBoxButtons.OK,
                result.Entity.State == EnumExtractedState.Unreadable ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        else
            ShowInfo(result.ErrorMessage, DocumentTexts.ReadDocument, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        await LoadTextAsync();
    }

    #region Transcribing the scanned pages (4.5b)

    private async void buttonTranscribe_Click(object sender, EventArgs e)
    {
        // While it runs, the button cancels.
        if (_transcribing != null)
        {
            _transcribing.Cancel();
            buttonTranscribe.Enabled = false;
            return;
        }
        await TranscribeAsync();
    }

    private async Task TranscribeAsync()
    {
        var scanned = await _ctrl.GetScannedPagesAsync(_resource);
        if (IsDisposed)
            return;
        if (!scanned.IsValid)
        {
            ShowInfo(scanned.ErrorMessage, DocumentTexts.TranscribePages, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (scanned.Entity == null)
        {
            ShowInfo(DocumentTexts.NoPagesToTranscribe, DocumentTexts.TranscribePages);
            return;
        }
        // The pages need a model that sees images, as the photos.
        var (provider, describer) = await _ctrl.GetPhotoDescriberAsync();
        if (IsDisposed)
            return;
        if (!describer.Available)
        {
            ShowInfo(describer.Message, DocumentTexts.TranscribePages, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var secondsPerPage = await _ctrl.GetSecondsPerPageAsync();
        if (IsDisposed)
            return;

        var withOcr = AskHowToTranscribe(scanned.Entity, describer, secondsPerPage);
        if (withOcr == null)
            return;
        var pages = scanned.Entity.PagesToTranscribe(withOcr.Value);

        _transcribing = new CancellationTokenSource();
        buttonTranscribe.Text = "Cancel";
        buttonRead.Enabled = false;
        progressLoad.Value = 0;
        progressLoad.Visible = true;
        (int Transcribed, List<string> Errors, bool Summarized) outcome;
        bool canceled;
        try
        {
            var progress = new Progress<(int Done, string Text)>(step =>
            {
                if (IsDisposed)
                    return;
                labelState.Text = step.Text;
                progressLoad.Value = pages.Count == 0 ? 100 : Math.Min(100, 100 * step.Done / pages.Count);
            });
            outcome = await _ctrl.TranscribePagesAsync(provider, _resource, pages, progress, _transcribing.Token);
        }
        finally
        {
            canceled = _transcribing.IsCancellationRequested;
            _transcribing.Dispose();
            _transcribing = null;
        }

        // The window may have been closed meanwhile.
        if (IsDisposed)
            return;
        buttonTranscribe.Text = DocumentTexts.TranscribePages;
        buttonTranscribe.Enabled = true;
        var summary = DocumentTexts.TranscriptionResult(outcome.Transcribed, pages.Count, canceled, outcome.Summarized);
        if (outcome.Errors.Count > 0)
            summary += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, outcome.Errors);
        ShowInfo(summary, DocumentTexts.TranscribePages, MessageBoxButtons.OK,
            outcome.Errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        await LoadTextAsync();
    }

    // How many pages, with which model, how long and whether it is paid; and, if some have an OCR layer, whether to include them.
    // Null: cancelled.
    private bool? AskHowToTranscribe(ScannedDocumentDto scanned, ResourceDescriberDto describer, double? secondsPerPage)
    {
        var all = scanned.PagesToTranscribe(withOcrLayer: true).Count;
        var text = DocumentTexts.TranscriptionConfirmation(all, describer, secondsPerPage);
        if (scanned.OcrPages.Count > 0)
            text += $"{Environment.NewLine}{Environment.NewLine}{scanned.OcrPages.Count} of them have an OCR layer: unchecking the box below leaves them out.";
        text += $"{Environment.NewLine}{Environment.NewLine}{DocumentTexts.AutomaticTranscriptionHelp}";

        var transcribe = new TaskDialogButton("Transcribe");
        var page = new TaskDialogPage
        {
            Caption = KntConst.AppName,
            Heading = DocumentTexts.TranscribePages,
            Text = text,
            Buttons = { transcribe, TaskDialogButton.Cancel },
            DefaultButton = transcribe
        };
        if (scanned.OcrPages.Count > 0)
            page.Verification = new TaskDialogVerificationCheckBox($"{DocumentTexts.WithOcrPages} ({scanned.OcrPages.Count})", true);

        if (TaskDialog.ShowDialog(this, page) != transcribe)
            return null;
        return page.Verification?.Checked ?? true;
    }

    #endregion

    #region The original page (4.5b)

    // The page where the cursor is.
    private void textDocument_CaretMoved(object sender, EventArgs e)
    {
        if (!_isPdf || DocumentTexts.PageAt(_readable, textDocument.SelectionStart) is not int number)
            return;
        numericPage.Value = Math.Clamp(number, (int)numericPage.Minimum, (int)numericPage.Maximum);
    }

    private void buttonOriginalPage_Click(object sender, EventArgs e)
    {
        var number = (int)numericPage.Value;
        var url = _ctrl.OriginalPageUrl(_resource, number);
        if (url == null)
        {
            ShowInfo(AiDocumentRules.FileNotAvailableMessage, DocumentTexts.SeeOriginalPage);
            return;
        }
        new DocumentPageForm(url, $"{_resource.GetTitle()} - {DocumentTexts.PageHeading(number)}").Show(this);
    }

    #endregion
}
