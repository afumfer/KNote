using System.Text;
using System.Text.RegularExpressions;

namespace KNote.ClientWin.Core.Reports;

// Proposed file names for what is saved from a report (PDF, CSV): readable, valid on Windows and short
// enough to leave room for the folder path.
public static class ReportFileName
{
    public const int MaxLength = 100;

    // Folder the save dialogs start in: the one last saved to (AppUserState.Reports.LastExportFolder) while
    // it still exists, the user's Documents folder otherwise.
    public static string InitialFolder(string lastExportFolder)
        => !string.IsNullOrEmpty(lastExportFolder) && Directory.Exists(lastExportFolder)
            ? lastExportFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public static string Sanitize(string text, string fallback = "KNote report")
    {
        if (string.IsNullOrWhiteSpace(text))
            return fallback;

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(text.Length);
        foreach (var c in text.Trim())
        {
            if (c == '\\' || c == '/')
                sb.Append('-');           // folder path separators read better as a dash
            else if (char.IsWhiteSpace(c))
                sb.Append(' ');
            else if (char.IsControl(c) || Array.IndexOf(invalid, c) >= 0)
                sb.Append('_');
            else
                sb.Append(c);
        }

        var name = Regex.Replace(sb.ToString(), @"\s+", " ");
        name = Regex.Replace(name, "-{2,}", "-");
        name = name.Trim(' ', '.', '-', '_');

        if (name.Length > MaxLength)
            name = name[..MaxLength].TrimEnd(' ', '.', '-', '_');

        return name.Length == 0 ? fallback : name;
    }
}
