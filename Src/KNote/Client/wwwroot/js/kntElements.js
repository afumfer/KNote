// Small DOM helpers for the Shared components.

// Bottom left corner of an element in the window (e.g. to open a menu below a button).
export function bottomLeft(element) {
    const rect = element.getBoundingClientRect();
    return { x: rect.left, y: rect.bottom };
}

// The text selected inside an element (e.g. the text of a document in the resource viewer), or "" if the selection is not in it.
// What has user-select: none (the headings of its pages) is not in it.
export function selectedTextIn(element) {
    const selection = window.getSelection();
    if (!element || !selection || selection.rangeCount === 0 || selection.isCollapsed)
        return "";
    const range = selection.getRangeAt(0);
    return element.contains(range.commonAncestorContainer) ? selection.toString() : "";
}
