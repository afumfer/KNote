namespace KntWebView
{
    /// <summary>
    /// The Mermaid diagrams of a note's HTML (the &lt;pre class="mermaid"&gt; Markdig writes for a ```mermaid block),
    /// drawn by mermaid.min.js and kntMermaid.js (the web app's copy, in Client/wwwroot/lib/mermaid). Drawing them
    /// needs scripts, which a note's HTML (written by users) doesn't run otherwise: the page that has them gets a
    /// Content-Security-Policy that only allows the scripts served from DiagramsHost, so none of the note's own
    /// could run even if one got past the HTML sanitizer.
    /// </summary>
    public static class KntDiagrams
    {
        /// <summary>The virtual host (WebView2's folder mapping) that serves the diagrams' scripts.</summary>
        public const string DiagramsHost = "knote.diagrams";

        /// <summary>Whether html has diagrams to draw.</summary>
        public static bool HasDiagrams(string? html) =>
            html != null && html.Contains("class=\"mermaid\"", StringComparison.Ordinal);

        /// <summary>
        /// The page that shows html with its diagrams drawn: the Content-Security-Policy first (a &lt;meta&gt; only
        /// applies to what comes after it) and the scripts last, once the diagrams' blocks are in the document.
        /// kntMermaid.js draws them itself (data-knt-render): the policy doesn't allow an inline script to call it.
        /// </summary>
        public static string Page(string html) =>
            $"<meta http-equiv=\"Content-Security-Policy\" content=\"script-src https://{DiagramsHost}/; object-src 'none'; base-uri 'none'\">"
            + html
            + $"<script src=\"https://{DiagramsHost}/mermaid.min.js\"></script>"
            + $"<script src=\"https://{DiagramsHost}/kntMermaid.js\" data-knt-render></script>";
    }
}
