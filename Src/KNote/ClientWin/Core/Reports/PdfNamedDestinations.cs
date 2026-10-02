using System.Text;
using System.Text.RegularExpressions;

namespace KNote.ClientWin.Core.Reports;

// Reads the page each named destination of a PDF points to. Chromium (the WebView2 print engine) writes
// an internal link target (<a href="#id"> -> element with that id) as a named destination of the PDF it
// prints, so this tells which page an element ended up on - the only way to get real page numbers for a
// table of contents, since Chromium supports neither target-counter() nor scripts in our reports.
// Works on the PDFs Chromium/Skia writes: classic (uncompressed) object dictionaries, a /Dests dictionary
// referenced from the catalog and a /Pages tree. Anything else yields an empty result (never throws), so
// callers degrade to a table of contents without page numbers.
public static class PdfNamedDestinations
{
    private static readonly Regex ObjectStart = new(@"(\d+)\s+0\s+obj\b", RegexOptions.Compiled);
    private static readonly Regex Reference = new(@"(\d+)\s+0\s+R\b", RegexOptions.Compiled);
    private static readonly Regex Destination = new(@"/([^\s/\[\]<>()]+)\s*\[\s*(\d+)\s+0\s+R", RegexOptions.Compiled);

    // Destination name -> 1-based page number.
    public static Dictionary<string, int> Parse(byte[] pdf)
    {
        var result = new Dictionary<string, int>();
        if (pdf == null || pdf.Length == 0)
            return result;

        try
        {
            var objects = ReadObjectDictionaries(Encoding.Latin1.GetString(pdf));

            var catalog = objects.Values.LastOrDefault(o => Regex.IsMatch(o, @"/Type\s*/Catalog\b"));
            if (catalog == null)
                return result;

            var pagesRoot = Regex.Match(catalog, @"/Pages\s+(\d+)\s+0\s+R");
            var destsRef = Regex.Match(catalog, @"/Dests\s+(\d+)\s+0\s+R");
            if (!pagesRoot.Success || !destsRef.Success)
                return result;

            var pageNumbers = new Dictionary<int, int>();
            CollectPages(objects, int.Parse(pagesRoot.Groups[1].Value), pageNumbers, new HashSet<int>());

            if (!objects.TryGetValue(int.Parse(destsRef.Groups[1].Value), out var dests))
                return result;

            foreach (Match match in Destination.Matches(dests))
                if (pageNumbers.TryGetValue(int.Parse(match.Groups[2].Value), out var page))
                    result[DecodeName(match.Groups[1].Value)] = page;
        }
        catch (Exception)
        {
            result.Clear();
        }

        return result;
    }

    // Object number -> its dictionary text (up to "stream" or "endobj"; stream data is never needed). A
    // later definition of the same object number (incremental update) replaces the earlier one.
    private static Dictionary<int, string> ReadObjectDictionaries(string text)
    {
        var objects = new Dictionary<int, string>();
        foreach (Match match in ObjectStart.Matches(text))
        {
            var start = match.Index + match.Length;
            var end = text.IndexOf("endobj", start, StringComparison.Ordinal);
            if (end < 0)
                continue;
            var stream = text.IndexOf("stream", start, end - start, StringComparison.Ordinal);
            objects[int.Parse(match.Groups[1].Value)] = text[start..(stream >= 0 ? stream : end)];
        }
        return objects;
    }

    // Walks the /Pages tree in document order, numbering its /Page leaves from 1.
    private static void CollectPages(Dictionary<int, string> objects, int objectNumber, Dictionary<int, int> pageNumbers, HashSet<int> visited)
    {
        if (!visited.Add(objectNumber) || !objects.TryGetValue(objectNumber, out var node))
            return;

        if (Regex.IsMatch(node, @"/Type\s*/Pages\b"))
        {
            var kids = Regex.Match(node, @"/Kids\s*\[([^\]]*)\]");
            if (kids.Success)
                foreach (Match kid in Reference.Matches(kids.Groups[1].Value))
                    CollectPages(objects, int.Parse(kid.Groups[1].Value), pageNumbers, visited);
        }
        else if (Regex.IsMatch(node, @"/Type\s*/Page\b"))
        {
            pageNumbers[objectNumber] = pageNumbers.Count + 1;
        }
    }

    // PDF name escapes: "#xx" is the byte with hex code xx.
    private static string DecodeName(string name)
        => Regex.Replace(name, "#([0-9A-Fa-f]{2})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
}
