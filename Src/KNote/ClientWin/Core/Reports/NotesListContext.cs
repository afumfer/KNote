namespace KNote.ClientWin.Core.Reports;

public enum NotesListSource
{
    Folder,
    Search,
    Filter
}

// Where the notes of the management window's list come from (a folder, a quick search or a structured
// filter), with every name already resolved (folder path, note type...). Built by KNoteManagementCtrl;
// turned here into the texts that identify the list in a printed report and in the exported file name.
public class NotesListContext
{
    public NotesListSource Source { get; set; }

    public string RepositoryAlias { get; set; }

    public string RepositoryProvider { get; set; }

    // Folder mode.
    public string FolderPath { get; set; }

    public int? FolderNumber { get; set; }

    // Search mode.
    public string SearchText { get; set; }

    public bool SearchInDescription { get; set; }

    public bool SearchInNoteTasks { get; set; }

    // Filter mode: the criteria actually set, as label/value (e.g. "Note type" / "Meeting").
    public List<ReportMetaItem> FilterCriteria { get; set; } = new();

    // In-memory text filter applied over the list (see NotesListSnapshot.TextFilter).
    public string TextFilter { get; set; }

    public string Repository
        => string.IsNullOrEmpty(RepositoryProvider) ? RepositoryAlias ?? "" : $"{RepositoryAlias} ({RepositoryProvider})";

    public string Heading => Source switch
    {
        NotesListSource.Folder => string.IsNullOrWhiteSpace(FolderPath) ? "(No folder selected)" : FolderPath,
        NotesListSource.Search => $"Search: {SearchText}",
        _ => $"Filter: {FilterCriteriaText(" · ")}"
    };

    public string Subheading
    {
        get
        {
            var parts = new List<string>();

            if (Source == NotesListSource.Folder && FolderNumber != null)
                parts.Add($"Folder number {FolderNumber}");
            else if (Source == NotesListSource.Search)
                parts.Add($"Searched in: {SearchScope}");

            if (!string.IsNullOrWhiteSpace(TextFilter))
                parts.Add($"List text filter: {TextFilter.Trim()}");

            return parts.Count > 0 ? string.Join(" · ", parts) : null;
        }
    }

    // Base of the proposed file name (without extension, not yet sanitized - see ReportFileName).
    public string FileNameBase(DateTime date, string prefix = "Notes")
    {
        var what = Source switch
        {
            NotesListSource.Folder => FolderPath,
            NotesListSource.Search => $"Search {SearchText}",
            _ => $"Filter {FilterCriteriaText(" ")}"
        };

        if (!string.IsNullOrWhiteSpace(TextFilter))
            what += $" ({TextFilter.Trim()})";

        return $"{prefix} - {what} - {date:yyyy-MM-dd}";
    }

    private string SearchScope
    {
        get
        {
            var scope = new List<string> { "topic" };
            if (SearchInDescription)
                scope.Add("description");
            if (SearchInNoteTasks)
                scope.Add("tasks");
            return string.Join(", ", scope);
        }
    }

    private string FilterCriteriaText(string separator)
        => FilterCriteria.Count == 0
            ? "(no criteria)"
            : string.Join(separator, FilterCriteria.Select(c => $"{c.Label} = {c.Value}"));
}
