using KNote.Model.Dto;

namespace KNote.Reports;

// Where a report finds the repository's resources (images and files of the notes), which depends on where it
// is shown: ClientWin's preview serves the resources folder from a WebView2 virtual host, the web app loads
// them from the server's resources URL.
public interface IReportResources
{
    // A description (Markdown or HTML) with its resource references pointing where the report can load them.
    string MapResources(string text);

    // The src of a resource's thumbnail (only for images), or null when it has none.
    string ThumbnailSource(ResourceDto resource);
}
