// Draws the Mermaid diagrams of a note: the <pre class="mermaid"> Markdig writes for a ```mermaid block (its
// text, escaped by KntHtmlSanitizer, is the diagram's definition). Shared by both apps' note viewers: the web
// app's ViewMarkdown and ClientWin's KntEditView, which loads it after mermaid.min.js (see KntDiagrams). A classic
// script, not a module, so that it can be loaded with a <script> in both.
(function () {
    var loading = null;
    var watching = false;
    // Drawings run one after the other: Mermaid's settings (the theme) are global.
    var queue = Promise.resolve();

    // The library: already there (ClientWin loads it first) or loaded once from libraryUrl (web app).
    function loadLibrary(libraryUrl) {
        if (window.mermaid)
            return Promise.resolve(window.mermaid);
        if (!libraryUrl)
            return Promise.reject(new Error("Mermaid is not loaded."));
        if (!loading) {
            loading = new Promise(function (resolve, reject) {
                var script = document.createElement("script");
                script.src = libraryUrl;
                script.onload = function () { resolve(window.mermaid); };
                script.onerror = function () {
                    loading = null;
                    reject(new Error("Mermaid could not be loaded from " + libraryUrl + "."));
                };
                document.head.appendChild(script);
            });
        }
        return loading;
    }

    // Whether a diagram goes on a dark background: its text is light. Followed from the text and not from the
    // app's theme, since a note can be shown on a page of its own color (e.g. the web app's white sheet for notes
    // with colors of their own in a dark theme).
    function isDark(node) {
        var color = getComputedStyle(node).color;
        var values = (color.match(/[\d.]+/g) || []).map(Number);
        if (values.length < 3)
            return false;
        // rgb(255, 255, 255) or color(srgb 1 1 1).
        var scale = color.indexOf("color(") === 0 ? 255 : 1;
        return (0.2126 * values[0] + 0.7152 * values[1] + 0.0722 * values[2]) * scale > 128;
    }

    // Draws nodes, all with the same theme (the one of the first). A diagram with errors keeps its text.
    function draw(nodes, libraryUrl) {
        var drawing = queue.then(function () { return drawNow(nodes, libraryUrl); });
        queue = drawing.catch(function () { });
        return drawing;
    }

    function drawNow(nodes, libraryUrl) {
        // Kept to draw it again (another theme) and to put it back if it can't be drawn (Mermaid empties it).
        nodes.forEach(function (node) {
            if (!node.hasAttribute("data-knt-source"))
                node.setAttribute("data-knt-source", node.textContent);
        });
        var dark = isDark(nodes[0]);

        return loadLibrary(libraryUrl).then(function (mermaid) {
            mermaid.initialize({
                startOnLoad: false,
                securityLevel: "strict",
                suppressErrorRendering: true,
                theme: dark ? "dark" : "default"
            });
            return mermaid.run({ nodes: nodes, suppressErrors: true });
        }).then(function () {
            nodes.forEach(function (node) {
                if (node.querySelector("svg"))
                    node.setAttribute("data-knt-dark", dark);
                else
                    node.textContent = node.getAttribute("data-knt-source");
            });
            return nodes.length;
        });
    }

    // Once the stylesheets of the page change (the web app's theme) and have loaded, draws again the diagrams
    // whose background is now light instead of dark or the other way round.
    function watchTheme(libraryUrl) {
        if (watching)
            return;
        watching = true;

        var pending = false;
        var check = function (waited) {
            var loaded = Array.prototype.every.call(document.querySelectorAll("link[rel='stylesheet']"),
                function (link) { return link.sheet; });
            if (!loaded && waited < 3000) {
                setTimeout(function () { check(waited + 50); }, 50);
                return;
            }
            pending = false;

            var changed = Array.prototype.filter.call(document.querySelectorAll("pre.mermaid[data-knt-dark]"),
                function (node) { return String(isDark(node)) !== node.getAttribute("data-knt-dark"); });
            changed.forEach(function (node) {
                node.textContent = node.getAttribute("data-knt-source");
                node.removeAttribute("data-processed");
                node.removeAttribute("data-knt-dark");
            });
            if (changed.length > 0)
                draw(changed, libraryUrl).catch(function (e) { console.error(e); });
        };

        new MutationObserver(function () {
            if (pending)
                return;
            pending = true;
            setTimeout(function () { check(0); }, 50);
        }).observe(document.head, { childList: true, subtree: true, attributes: true, attributeFilter: ["href"] });
    }

    window.kntMermaid = {
        // Draws the diagrams under root (the whole document by default) not drawn yet, and resolves to how many
        // it found. libraryUrl: where to load mermaid.min.js from when it isn't loaded yet.
        render: function (root, libraryUrl) {
            var nodes = Array.prototype.filter.call((root || document).querySelectorAll("pre.mermaid"),
                function (node) { return !node.hasAttribute("data-processed"); });
            if (nodes.length === 0)
                return Promise.resolve(0);

            watchTheme(libraryUrl);
            return draw(nodes, libraryUrl);
        }
    };

    // Loaded with data-knt-render (ClientWin, see KntDiagrams): draws the page's diagrams by itself, since the
    // page's Content-Security-Policy doesn't allow an inline script to call render.
    var current = document.currentScript;
    if (current && current.hasAttribute("data-knt-render")) {
        var renderPage = function () {
            window.kntMermaid.render().catch(function (e) { console.error(e); });
        };
        if (document.readyState === "loading")
            document.addEventListener("DOMContentLoaded", renderPage);
        else
            renderPage();
    }
})();
