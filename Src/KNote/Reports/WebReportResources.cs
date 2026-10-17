using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Reports;

// Resources for a report shown in a browser: loaded from the URLs of the resources folder served by the web
// server (RepositoryRef.ResourcesContainerRootUrl), as the web app shows them in a note.
public class WebReportResources : IReportResources
{
    private readonly IKntService _service;

    public WebReportResources(IKntService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public string MapResources(string text)
        => _service.Notes.UtilUpdateResourceInDescriptionForRead(text);

    // The resource's URL (set by the service when it reads the note's resources), or a data: URI when there is
    // none and its content is in memory.
    public string ThumbnailSource(ResourceDto resource)
    {
        if (resource.FileType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
            return null;

        if (!string.IsNullOrEmpty(resource.FullUrl))
            return resource.FullUrl;

        if (resource.ContentArrayBytes?.Length > 0)
            return $"data:{resource.FileType};base64,{Convert.ToBase64String(resource.ContentArrayBytes)}";

        return null;
    }
}
