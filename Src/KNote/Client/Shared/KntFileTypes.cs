namespace KNote.Client.Shared;

/// <summary>
/// The kind of a file by its MIME type (a note's resources): whether it is an image, and a Material Symbols icon
/// for it. No images from other sites: the icons come with Radzen's font.
/// </summary>
public static class KntFileTypes
{
    // How the browser can show a file (Notes/NoteResourceViewer); Other: it can't (Word, zip...).
    public enum Kind { Image, Pdf, Video, Audio, Text, Other }

    // By extension, for the files whose type is not saved (ClientWin only saves it for a few: jpg, png, pdf...).
    private static readonly Dictionary<string, Kind> KindsByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = Kind.Image, [".jpeg"] = Kind.Image, [".png"] = Kind.Image, [".gif"] = Kind.Image,
        [".bmp"] = Kind.Image, [".webp"] = Kind.Image, [".svg"] = Kind.Image, [".ico"] = Kind.Image,
        [".pdf"] = Kind.Pdf,
        [".mp4"] = Kind.Video, [".webm"] = Kind.Video, [".ogv"] = Kind.Video, [".mov"] = Kind.Video,
        [".mp3"] = Kind.Audio, [".wav"] = Kind.Audio, [".ogg"] = Kind.Audio, [".m4a"] = Kind.Audio,
        // Only text the web server serves as such (a .csv is downloaded, a .md or .log is not served).
        [".txt"] = Kind.Text, [".text"] = Kind.Text, [".json"] = Kind.Text
    };

    /// <summary>How a file can be shown: by its type (MIME) or, without one the browser can show, by its name.</summary>
    public static Kind KindOf(string? fileType, string? name = null)
    {
        var type = fileType?.ToLowerInvariant() ?? "";
        if (type.StartsWith("image/")) return Kind.Image;
        if (type == "application/pdf") return Kind.Pdf;
        if (type.StartsWith("video/")) return Kind.Video;
        if (type.StartsWith("audio/")) return Kind.Audio;
        if (type == "text/plain") return Kind.Text;

        var extension = System.IO.Path.GetExtension(name ?? "");
        return KindsByExtension.TryGetValue(extension, out var kind) ? kind : Kind.Other;
    }

    public static bool IsImage(string? fileType, string? name = null) => KindOf(fileType, name) == Kind.Image;

    public static string Icon(string? fileType, string? name = null)
    {
        switch (KindOf(fileType, name))
        {
            case Kind.Image: return "image";
            case Kind.Pdf: return "picture_as_pdf";
            case Kind.Audio: return "audio_file";
            case Kind.Video: return "video_file";
            case Kind.Text: return "description";
        }

        var type = fileType?.ToLowerInvariant() ?? "";
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
