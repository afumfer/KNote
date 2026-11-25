using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// A version of a note (the history, H.3, «See this version»), in a window of its own, read only: the note as it was after a change
/// of its history (H18), with its names resolved by the Service and without its script (H21). If copies in between were not kept,
/// it says so (Notice). Included in the note editor (no [KntAuthorize]): the command checks the role (Guest).
/// </summary>
public class NoteVersionCtrl : CtrlViewBase<IViewNoteVersion>
{
    #region Constructor

    // restore: «Restore» of this version (H23, the note editor, which asks first); null, no «Restore».
    public NoteVersionCtrl(Store store, IKntService service, Guid noteId, KLogDto row, string noteTitle, Func<Task<bool>> restore = null) : base(store)
    {
        ControllerName = "Note version";
        Service = service;
        NoteId = noteId;
        Row = row;
        NoteTitle = noteTitle;
        _restore = restore;
    }

    private readonly Func<Task<bool>> _restore;

    #endregion

    #region Properties

    public IKntService Service { get; }
    public Guid NoteId { get; }
    public KLogDto Row { get; }
    public string NoteTitle { get; }

    public NoteVersionViewDto Version { get; private set; }

    public string Title => string.IsNullOrEmpty(NoteTitle) ? "Version of the note" : $"Version of {NoteTitle}";

    public string Subtitle => $"As it was after the change of {HistoryRules.WhenText(Row)}"
        + (string.IsNullOrEmpty(Row?.UserName) ? "" : $" by {Row.UserName}");

    #endregion

    #region Controller implementation

    protected override IViewNoteVersion CreateView() =>
        Store.FactoryViews.Registry.Resolve<NoteVersionCtrl, IViewNoteVersion>(this);

    #endregion

    #region Public methods

    /// <summary>Reads the version; false (with its error in Error) if it can't be read.</summary>
    public async Task<bool> LoadAsync()
    {
        Error = null;
        var result = await Service.History.GetNoteVersionAsync(NoteId, Row.KLogId);
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Version = result.Entity;
        return true;
    }

    public string Error { get; private set; }

    public bool CanRestore => _restore != null;

    /// <summary>«Restore» (H23): true if the note was restored (the window then closes).</summary>
    public async Task<bool> RestoreAsync() => _restore != null && await _restore();

    /// <summary>
    /// The HTML of its description, as the note editor shows a note's (made safe by the Service, with its resources through the
    /// virtual host of the web view), or null without a description.
    /// </summary>
    public string DescriptionHtml()
    {
        if (string.IsNullOrWhiteSpace(Version?.Description))
            return null;
        var description = Service.Notes.UtilUpdateResourceInDescriptionForRead(Version.Description, true);
        if (Version.DescriptionFormat == "html")
            return Service.Notes.UtilSanitizeHtml(description);
        var rootUrl = Service.RepositoryRef?.ResourcesContainerRootUrl;
        if (!string.IsNullOrEmpty(rootUrl))
            description = description.Replace(rootUrl, KntConst.VirtualHostNameToFolderMapping);
        return Service.Notes.UtilMarkdownToHtml(description);
    }

    /// <summary>Where the resources of the note are, for the web view that shows its description.</summary>
    public string ResourcesFolder => Service.RepositoryRef?.ResourcesContainerRootPath;

    #endregion
}
