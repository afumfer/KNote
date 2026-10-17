using System.Globalization;
using KNote.Model;
using KNote.Reports;

namespace KNote.ClientWin.Core;

/// <summary>
/// Pure text/file parsing helpers extracted from <see cref="Store"/> (Fase 1 of the ClientWin
/// architecture refactor, see ClientWin/CLAUDE.md). Exposed as the lazily-created singleton
/// <see cref="Store.KntTextUtils"/> property (later refactor step) rather than through wrapper
/// methods on Store, so callers use Store.KntTextUtils.Method(...) directly.
/// </summary>
public class KntTextUtils
{
    public DateTime? TextToDateTime(string text)
    {
        DateTime output;
        if (DateTime.TryParse(text, out output))
            return output;
        else
            return null;
    }

    public int TextToInt(string text)
    {
        int output;
        if (int.TryParse(text, out output))
            return output;
        else
            return 0;
    }

    // Accepts both "." and "," as the decimal separator, whatever the current culture: these values
    // (trace note weights, task times, difficulty levels) never need a thousands separator, and the
    // culture's default parsing silently read "1.5" typed under es-ES as 15 ("." being its thousands
    // separator). Text with both separators (e.g. "1.500,25") is rejected instead of misread.
    public double? TextToDouble(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var normalized = text.Trim().Replace(".", decimalSeparator).Replace(",", decimalSeparator);

        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out var output))
            return output;
        else
            return null;
    }

    // Shared with the printed reports (KNote.Reports), which show these notes as a link.
    public string ExtractUrlFromText(string text)
        => NoteDescriptions.ExtractUrl(text);

    public string ExtensionFileToFileType(string extension)
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

    public bool IsSupportedFileTypeForPreview(string fileType)
    {
        // TODO: Refactor this method
        if (string.IsNullOrEmpty(fileType))
            return false;

        return KntConst.SupportedMimeTypes.Contains(fileType);
    }
}
