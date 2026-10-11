// Theme saved in the browser (cookies), read and written by App.razor (kntTheme.read / kntTheme.save), and the
// Home header style (kntTheme.getHomeHeader / setHomeHeader, from the Appearance panel, KntThemeSelector).
// This script runs in <head> before Blazor starts and loads the saved theme's stylesheet at once, so the page
// doesn't flash the default theme while the app loads. RadzenTheme (App.razor) then adds its own stylesheet,
// and these preloaded ones are removed as soon as that one has loaded.
(function () {
    var themeCookie = "KNoteTheme";
    var wcagCookie = "KNoteThemeWcag";
    var versionCookie = "KNoteThemeVersion";
    var homeHeaderCookie = "KNoteHomeHeader";
    var homeHeaderStyles = ["soft", "ink"];  // the first one is the default
    var maxAge = 365 * 24 * 60 * 60;

    function getCookie(name) {
        var match = document.cookie.match(new RegExp("(?:^|; )" + name + "=([^;]*)"));
        return match ? decodeURIComponent(match[1]) : null;
    }

    function setCookie(name, value) {
        document.cookie = name + "=" + encodeURIComponent(value || "") + "; max-age=" + maxAge + "; path=/; SameSite=Lax";
    }

    // Only theme names (App.razor checks them against the known themes too).
    function validName(theme) {
        return theme && /^[a-z-]+$/.test(theme) ? theme : null;
    }

    window.kntTheme = {
        read: function () {
            return { theme: validName(getCookie(themeCookie)), wcag: getCookie(wcagCookie) === "true" };
        },
        save: function (theme, wcag, version) {
            setCookie(themeCookie, theme);
            setCookie(wcagCookie, wcag ? "true" : "false");
            setCookie(versionCookie, version);
        },
        // Style of the Home header (Pages/Index.razor.css), as data-knt-home-header on <html>.
        getHomeHeader: function () {
            var style = getCookie(homeHeaderCookie);
            return homeHeaderStyles.indexOf(style) >= 0 ? style : homeHeaderStyles[0];
        },
        setHomeHeader: function (style) {
            if (homeHeaderStyles.indexOf(style) < 0)
                return;
            setCookie(homeHeaderCookie, style);
            document.documentElement.setAttribute("data-knt-home-header", style);
        }
    };

    document.documentElement.setAttribute("data-knt-home-header", window.kntTheme.getHomeHeader());

    var saved = window.kntTheme.read();
    if (!saved.theme)
        return;

    // Same URLs as RadzenTheme (ThemeService.Href / WcagHref), so the browser reuses the downloaded files.
    var version = getCookie(versionCookie);
    var query = version ? "?v=" + encodeURIComponent(version) : "";
    var preloaded = [];

    function preload(file) {
        var link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = "_content/Radzen.Blazor/css/" + file + query;
        document.head.appendChild(link);
        preloaded.push(link);
    }

    preload(saved.theme + "-base.css");
    if (saved.wcag)
        preload(saved.theme + "-wcag.css");

    new MutationObserver(function (_, observer) {
        var themeLink = document.getElementById("radzen-theme-link");
        if (!themeLink)
            return;
        observer.disconnect();
        var removePreloaded = function () { preloaded.forEach(function (link) { link.remove(); }); };
        if (themeLink.sheet)
            removePreloaded();
        else {
            themeLink.addEventListener("load", removePreloaded);
            themeLink.addEventListener("error", removePreloaded);
        }
    }).observe(document.head, { childList: true });
})();
