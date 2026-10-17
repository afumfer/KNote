namespace KNote.Reports;

public static class NoteDescriptions
{
    private static readonly char[] NewLine = { '\r', '\n' };

    // The web page of a "navigation" description: its first line, when that is an http, https or file URL.
    public static string ExtractUrl(string text)
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
}
