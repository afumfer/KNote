using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Pure text/file parsing helpers extracted from <see cref="Store"/> (Fase 1 of the ClientWin
/// architecture refactor, see ClientWin/CLAUDE.md). Behavior is unchanged from the original
/// Store methods, including existing quirks (e.g. TextToDouble depends on CurrentCulture).
/// </summary>
public static class KntTextUtils
{
    private static readonly char[] NewLine = { '\r', '\n' };

    public static DateTime? TextToDateTime(string text)
    {
        DateTime output;
        if (DateTime.TryParse(text, out output))
            return output;
        else
            return null;
    }

    public static int TextToInt(string text)
    {
        int output;
        if (int.TryParse(text, out output))
            return output;
        else
            return 0;
    }

    public static double? TextToDouble(string text)
    {
        double output;
        if (double.TryParse(text, out output))
            return output;
        else
            return null;
    }

    public static string ExtractUrlFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        int indexJump = text.IndexOfAny(NewLine);
        var urlFistLine = (indexJump >= 0) ? text.Substring(0, indexJump) : text;

        Uri resultUri;
        var validResult = Uri.TryCreate(urlFistLine, UriKind.Absolute, out resultUri) &&
               (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps || resultUri.Scheme == Uri.UriSchemeFile);

        if (validResult)
            return urlFistLine;
        else
            return null;
    }

    public static string ExtensionFileToFileType(string extension)
    {
        // TODO: Refactor this method

        var ext = extension.ToLower();

        if (ext == ".jpg")
            return @"image/jpeg";
        else if (ext == ".jpeg")
            return @"image/jpeg";
        else if (ext == ".png")
            return "image/png";
        else if (ext == ".pdf")
            return "application/pdf";
        else if (ext == ".mp4")
            return "video/mp4";
        else if (ext == ".mp3")
            return "audio/mp3";
        else if (ext == ".txt")
            return "text/plain";
        else if (ext == ".text")
            return "text/plain";
        else if (ext == ".htm")
            return "text/plain";
        else if (ext == ".html")
            return "text/plain";
        else
            return "";
    }

    public static bool IsSupportedFileTypeForPreview(string fileType)
    {
        // TODO: Refactor this method
        if (string.IsNullOrEmpty(fileType))
            return false;

        return KntConst.SupportedMimeTypes.Contains(fileType);
    }
}
