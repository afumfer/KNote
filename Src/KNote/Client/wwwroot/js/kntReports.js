// Printable reports (Shared/KntReportPreview) and exported files.
//
// A report is a whole HTML page (with its own styles and A4 page setup) shown in a sandboxed iframe: no scripts run
// in it (its HTML comes from the notes, already made safe by the server), and "allow-same-origin" only lets this
// module reach it, to print it and fix its links. It is loaded from a blob: URL rather than srcdoc, so that the
// links of a book's contents ("#knt-ch-1") stay inside the report: a srcdoc page resolves them against the app's
// URL and would navigate away from it.

// Shows the html in the iframe; title is its document's title (the name the browser proposes when saving it as
// PDF). Returns the blob: URL, to release when the report is closed.
export function showReport(iframe, html, title) {
    const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
    iframe.addEventListener('load', () => {
        const doc = iframe.contentDocument;
        if (!doc)
            return;
        if (title)
            doc.title = title;
        // Links to other pages (e.g. a "navigation" note's web page) open in a new tab, not inside the report.
        for (const link of doc.querySelectorAll('a[href]')) {
            if (!link.getAttribute('href').startsWith('#')) {
                link.target = '_blank';
                link.rel = 'noopener noreferrer';
            }
        }
    }, { once: true });
    iframe.src = url;
    return url;
}

// The browser's print dialog for the report only (also "Save as PDF").
export function printReport(iframe) {
    const win = iframe.contentWindow;
    if (!win)
        return false;
    win.focus();
    win.print();
    return true;
}

export function releaseReport(url) {
    if (url)
        URL.revokeObjectURL(url);
}

// Saves content (text) as a file called fileName, through the browser's downloads.
export function downloadFile(fileName, contentType, content) {
    const url = URL.createObjectURL(new Blob([content], { type: contentType }));
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    // Released once the download has started.
    setTimeout(() => URL.revokeObjectURL(url), 10000);
}
