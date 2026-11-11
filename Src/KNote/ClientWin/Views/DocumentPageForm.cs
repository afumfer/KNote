using KNote.Model;

namespace KNote.ClientWin.Views;

// KNote Atlas, stage 4 (4.5b): «See the original page» of DocumentTextForm, the PDF at one page in a window of the app (the PDF
// viewer of WebView2, as the preview of the resources), to compare the page with its automatic transcription. Not modal: the text
// stays beside it.
public class DocumentPageForm : KntForm
{
    private readonly KntWebView.KntEditView _view;
    private readonly string _url;

    public DocumentPageForm(string url, string title)
    {
        _url = url;
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        ClientSize = new Size(LogicalToDeviceUnits(760), LogicalToDeviceUnits(900));

        _view = new KntWebView.KntEditView
        {
            Dock = DockStyle.Fill,
            ShowStatusInfo = false,
            ShowNavigationTools = false
        };
        Controls.Add(_view);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            await _view.ShowNavigationUrlContent(_url);
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                ShowInfo(ex.Message, KntConst.AppName);
        }
    }
}
