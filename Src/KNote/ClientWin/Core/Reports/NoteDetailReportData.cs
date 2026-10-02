using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core.Reports;

// A resource of the note as shown in the report. ThumbnailSrc is set only for images: a
// KntConst.VirtualHostNameToFolderMapping URL when the file is in the repository's resources folder, or a
// data: URI when its content is only available in memory (e.g. stored in the database, not cached yet).
public record NoteDetailResource(int Order, string Name, string FileType, string Description, string ThumbnailSrc);

public record NoteDetailTask(NoteTaskDto Task, string DescriptionHtml);

// Everything the note details report shows, already resolved and rendered (folder path, description and
// task descriptions as HTML, trace notes). Built by CreateAsync from a NoteExtendedDto; NoteDetailReport
// turns it into the report. The note's Script, its attributes' scripts and its alarms are never included.
public class NoteDetailReportData
{
    public NoteExtendedDto Note { get; set; }

    public string FolderPath { get; set; }

    public string Repository { get; set; }

    // Description rendered as HTML (null when empty or when it is a web page link, see DescriptionUrl).
    public string DescriptionHtml { get; set; }

    // "Navigation" notes whose description is a web page: the page's URL (its content is not printed).
    public string DescriptionUrl { get; set; }

    public List<NoteDetailResource> Resources { get; set; } = new();

    public List<NoteDetailTask> Tasks { get; set; } = new();

    public List<TraceNoteRow> TraceNotesFrom { get; set; } = new();

    public List<TraceNoteRow> TraceNotesTo { get; set; } = new();

    // Printed from the editor with changes not saved yet.
    public bool UnsavedChanges { get; set; }

    public static async Task<NoteDetailReportData> CreateAsync(Store store, ServiceRef serviceRef, NoteExtendedDto note, bool unsavedChanges)
    {
        ArgumentNullException.ThrowIfNull(serviceRef);
        ArgumentNullException.ThrowIfNull(note);

        var service = serviceRef.Service;
        var repositoryRef = serviceRef.RepositoryRef;

        var data = new NoteDetailReportData
        {
            Note = note,
            Repository = string.IsNullOrEmpty(repositoryRef?.Provider) ? repositoryRef?.Alias : $"{repositoryRef.Alias} ({repositoryRef.Provider})",
            UnsavedChanges = unsavedChanges,
            FolderPath = note.FolderId == Guid.Empty ? "" : await store.GetKNoteFolerPath(serviceRef, note.FolderId)
        };

        (data.DescriptionHtml, data.DescriptionUrl) = RenderDescription(store, service, repositoryRef, note);

        foreach (var resource in note.Resources.Where(r => !r.IsDeleted()).OrderBy(r => r.Order))
            data.Resources.Add(new NoteDetailResource(resource.Order, resource.NameOut, resource.FileType, resource.Description,
                ThumbnailSource(service, repositoryRef, resource)));

        foreach (var task in note.Tasks.Where(t => !t.IsDeleted()))
            data.Tasks.Add(new NoteDetailTask(task, MarkdownToHtml(service, repositoryRef, task.Description)));

        var typeNames = await TraceNoteRows.GetTypeNamesAsync(service);
        data.TraceNotesFrom = await TraceNoteRows.ResolveAllAsync(service, note.TraceNotesFrom, fromSide: true, typeNames);
        data.TraceNotesTo = await TraceNoteRows.ResolveAllAsync(service, note.TraceNotesTo, fromSide: false, typeNames);

        return data;
    }

    // A note's description as printable HTML, with the same rules as the note editor's view of it
    // (NoteEditorForm.ModelToControls): html is shown as is, "navigation" is either a web page (only its
    // URL is returned, not rendered) or markdown, anything else is markdown. Both null when it is empty.
    // Resource references point to KntConst.VirtualHostNameToFolderMapping (see ReportPreviewCtrl).
    public static (string Html, string Url) RenderDescription(Store store, IKntService service, RepositoryRef repositoryRef, NoteInfoDto note)
    {
        var description = note?.Description;
        if (string.IsNullOrWhiteSpace(description))
            return (null, null);

        var contentType = note.GetContentTypeExt().ForDescription;
        var url = contentType == "navigation" ? store.KntTextUtils.ExtractUrlFromText(description) : null;

        if (contentType == "html")
            return (MapResources(repositoryRef, description), null);
        if (!string.IsNullOrEmpty(url))
            return (null, url);
        return (MarkdownToHtml(service, repositoryRef, description), null);
    }

    private static string MarkdownToHtml(IKntService service, RepositoryRef repositoryRef, string markdown)
        => string.IsNullOrWhiteSpace(markdown) ? null : service.Notes.UtilMarkdownToHtml(MapResources(repositoryRef, markdown));

    // Stored descriptions reference resources as "<ResourcesContainer>/<subfolder>/<file>" (see
    // IKntNoteService.UtilUpdateResourceInDescriptionForWrite); the report preview serves the resources
    // root folder under KntConst.VirtualHostNameToFolderMapping.
    private static string MapResources(RepositoryRef repositoryRef, string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(repositoryRef?.ResourcesContainer))
            return text;

        return text.Replace(repositoryRef.ResourcesContainer, $"{KntConst.VirtualHostNameToFolderMapping}/{repositoryRef.ResourcesContainer}");
    }

    private static string ThumbnailSource(IKntService service, RepositoryRef repositoryRef, ResourceDto resource)
    {
        if (resource.FileType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
            return null;

        var rootPath = repositoryRef?.ResourcesContainerRootPath;
        var filePath = service.Notes.UtilGetResourceFilePath(resource);
        if (!string.IsNullOrEmpty(rootPath) && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            var relative = Path.GetRelativePath(rootPath, filePath);
            if (!relative.StartsWith("..") && !Path.IsPathRooted(relative))
                return $"{KntConst.VirtualHostNameToFolderMapping}/{string.Join('/', relative.Split('\\', '/').Select(Uri.EscapeDataString))}";
        }

        if (resource.ContentArrayBytes?.Length > 0)
            return $"data:{resource.FileType};base64,{Convert.ToBase64String(resource.ContentArrayBytes)}";

        return null;
    }
}
