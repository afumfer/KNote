namespace KNote.Client.Shared;

/// <summary>
/// The kind of a file by its MIME type (a note's resources): whether it is an image, and a Material Symbols icon
/// for it. No images from other sites: the icons come with Radzen's font.
/// </summary>
public static class KntFileTypes
{
    public static bool IsImage(string? fileType) =>
        fileType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

    public static string Icon(string? fileType)
    {
        var type = fileType?.ToLowerInvariant() ?? "";
        if (type.StartsWith("image/")) return "image";
        if (type == "application/pdf") return "picture_as_pdf";
        if (type.StartsWith("audio/")) return "audio_file";
        if (type.StartsWith("video/")) return "video_file";
        if (type.StartsWith("text/")) return "description";
        if (type.Contains("zip") || type.Contains("compressed") || type.Contains("tar")) return "folder_zip";
        if (type.Contains("sheet") || type.Contains("excel") || type.Contains("csv")) return "table_chart";
        if (type.Contains("presentation") || type.Contains("powerpoint")) return "slideshow";
        if (type.Contains("word") || type.Contains("document")) return "article";
        return "draft";
    }

    /// <summary>A size for people (e.g. 1.4 MB).</summary>
    public static string Size(long bytes) =>
        bytes >= 1024 * 1024 ? $"{bytes / 1024d / 1024d:0.#} MB" : bytes >= 1024 ? $"{bytes / 1024d:0} KB" : $"{bytes} bytes";
}
