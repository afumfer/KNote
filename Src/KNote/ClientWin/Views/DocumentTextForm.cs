using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// KNote Atlas, stage 4 (4.4b): the text KNote read of a document of the note, whole (decided by the owner: the Web shows its
// beginning), with its pages and the scanned ones marked, put in the box in pieces with a progress bar (a document can have
// thousands of pages). «Read document» / «Read again» reads it now (Staff: the command checks it), asking first if it was read.
public partial class DocumentTextForm : KntForm
{
    // What is put in the box at a time, so that the window keeps answering and the bar moves.
    private const int PieceLength = 256 * 1024;

    private readonly NoteEditorCtrl _ctrl;
    private readonly ResourceDto _resource;
    private readonly bool _paged;
    private EnumExtractedState _state;
    private CancellationTokenSource _loading;

    public DocumentTextForm()
    {
        InitializeComponent();
    }

    public DocumentTextForm(NoteEditorCtrl ctrl, ResourceDto resource) : this()
    {
        _ctrl = ctrl;
        _resource = resource;
        _paged = DocumentTextRules.IsPaged(DocumentTextRules.FormatOf(resource.FileType, resource.Name));
        Text = $"{resource.GetTitle()} - text";
        labelFile.Text = resource.NameOut;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadTextAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _loading?.Cancel();
        base.OnFormClosing(e);
    }

    private async Task LoadTextAsync()
    {
        _loading?.Cancel();
        var loading = _loading = new CancellationTokenSource();

        textDocument.Clear();
        buttonRead.Enabled = false;
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

        progressLoad.Visible = false;
        var state = DocumentTexts.State(_state);
        if (_state == EnumExtractedState.Partial)
            state += " " + DocumentTexts.ScannedPageHelp;
        else if (_state != EnumExtractedState.NotRead && text.Length == 0)
            state += " " + DocumentTexts.NothingRead;
        labelState.Text = state;
        buttonRead.Text = _state == EnumExtractedState.NotRead ? DocumentTexts.ReadDocument : DocumentTexts.ReadAgain;
        buttonRead.Enabled = true;
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
}
