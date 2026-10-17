using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Reports;

// A resource of the note as shown in the report. ThumbnailSrc is set only for images (see
// IReportResources.ThumbnailSource).
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

    public static async Task<NoteDetailReportData> CreateAsync(IKntService service, RepositoryRef repositoryRef, IReportResources resources,
        NoteExtendedDto note, bool unsavedChanges, ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(note);

        var data = new NoteDetailReportData
        {
            Note = note,
            Repository = string.IsNullOrEmpty(repositoryRef?.Provider) ? repositoryRef?.Name : $"{repositoryRef.Name} ({repositoryRef.Provider})",
            UnsavedChanges = unsavedChanges,
            FolderPath = note.FolderId == Guid.Empty ? "" : await GetFolderPathAsync(service, note.FolderId, logger)
        };

        (data.DescriptionHtml, data.DescriptionUrl) = RenderDescription(service, resources, note);

        foreach (var resource in note.Resources.Where(r => !r.IsDeleted()).OrderBy(r => r.Order))
            data.Resources.Add(new NoteDetailResource(resource.Order, resource.NameOut, resource.FileType, resource.Description,
                resources.ThumbnailSource(resource)));

        foreach (var task in note.Tasks.Where(t => !t.IsDeleted()))
            data.Tasks.Add(new NoteDetailTask(task, MarkdownToHtml(service, resources, task.Description)));

        var typeNames = await TraceNoteRows.GetTypeNamesAsync(service);
        data.TraceNotesFrom = await TraceNoteRows.ResolveAllAsync(service, note.TraceNotesFrom, fromSide: true, typeNames);
        data.TraceNotesTo = await TraceNoteRows.ResolveAllAsync(service, note.TraceNotesTo, fromSide: false, typeNames);

        return data;
    }

    // A note's description as printable HTML, with the same rules as ClientWin's note editor view of it
    // (NoteEditorForm.ModelToControls): html is made safe (UtilSanitizeHtml), "navigation" is either a web page
    // (only its URL is returned, not rendered) or markdown, anything else is markdown (UtilMarkdownToHtml, also
    // made safe). Both null when it is empty. Resource references point where the report loads them
    // (IReportResources.MapResources).
    public static (string Html, string Url) RenderDescription(IKntService service, IReportResources resources, NoteInfoDto note)
    {
        var description = note?.Description;
        if (string.IsNullOrWhiteSpace(description))
            return (null, null);

        var contentType = note.GetContentTypeExt().ForDescription;
        var url = contentType == "navigation" ? NoteDescriptions.ExtractUrl(description) : null;

        if (contentType == "html")
            return (service.Notes.UtilSanitizeHtml(resources.MapResources(description)), null);
        if (!string.IsNullOrEmpty(url))
            return (null, url);
        return (MarkdownToHtml(service, resources, description), null);
    }

    // The folder's full path (e.g. "Home\Projects"), or empty when it can't be read.
    public static async Task<string> GetFolderPathAsync(IKntService service, Guid folderId, ILogger logger = null)
    {
        var res = await service.Folders.GetPathAsync(folderId);
        if (!res.IsValid)
            logger?.LogError("GetFolderPathAsync: {message}", res.ErrorMessage);
        return res.IsValid ? res.Entity : string.Empty;
    }

    private static string MarkdownToHtml(IKntService service, IReportResources resources, string markdown)
        => string.IsNullOrWhiteSpace(markdown) ? null : service.Notes.UtilMarkdownToHtml(resources.MapResources(markdown));
}
