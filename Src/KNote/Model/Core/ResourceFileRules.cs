namespace KNote.Model;

/// <summary>
/// The files of the notes' resources (KNote Atlas, stage 4, D6): up to KntConst.MaxLenNoteResourceFile in both apps, also when
/// they are kept in the database. The attachments of the assistant and the images of the chat room have their own limit.
/// </summary>
public static class ResourceFileRules
{
    /// <summary>The limit as the apps say it («200 MB»).</summary>
    public static string MaxSizeText => $"{KntConst.MaxLenNoteResourceFile / (1024 * 1024)} MB";

    /// <summary>Why a file can't be a resource of a note (empty or too big), or null.</summary>
    public static string SizeError(string fileName, long length) =>
        length <= 0 ? $"{fileName} is empty."
        : length > KntConst.MaxLenNoteResourceFile ? $"{fileName} is too big: {MaxSizeText} at most."
        : null;
}
