namespace KNote.Client.Pages.Local;

/// <summary>How a repository is added to KNote Local's computer (LocalRepositoryAdd).</summary>
public enum LocalRepositoryKind
{
    // An empty SQLite database in the data folder.
    New,
    // A SQLite database of the computer (e.g. the desktop app's).
    Existing,
    // A SQL Server database.
    SqlServer
}
