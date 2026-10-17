using KNote.Model;
using KNote.Model.Dto;
using KNote.Reports;
using KNote.Service.Core;

namespace KNote.ClientWin.Core.Reports;

// The report preview (ReportPreviewForm, WebView2) serves the repository's resources root folder under
// KntConst.VirtualHostNameToFolderMapping: descriptions and thumbnails point there.
public class ClientWinReportResources : IReportResources
{
    private readonly IKntService _service;
    private readonly RepositoryRef _repositoryRef;

    public ClientWinReportResources(IKntService service, RepositoryRef repositoryRef)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _repositoryRef = repositoryRef;
    }

    public ClientWinReportResources(ServiceRef serviceRef)
        : this(serviceRef?.Service, serviceRef?.RepositoryRef)
    {
    }

    // Stored descriptions reference resources as "<ResourcesContainer>/<subfolder>/<file>" (see
    // IKntNoteService.UtilUpdateResourceInDescriptionForWrite).
    public string MapResources(string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(_repositoryRef?.ResourcesContainer))
            return text;

        return text.Replace(_repositoryRef.ResourcesContainer, $"{KntConst.VirtualHostNameToFolderMapping}/{_repositoryRef.ResourcesContainer}");
    }

    // A virtual host URL when the file is in the repository's resources folder, or a data: URI when its
    // content is only available in memory (e.g. stored in the database, not cached yet).
    public string ThumbnailSource(ResourceDto resource)
    {
        if (resource.FileType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
            return null;

        var rootPath = _repositoryRef?.ResourcesContainerRootPath;
        var filePath = _service.Notes.UtilGetResourceFilePath(resource);
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
