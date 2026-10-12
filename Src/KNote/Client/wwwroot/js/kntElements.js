// Small DOM helpers for the Shared components.

// Bottom left corner of an element in the window (e.g. to open a menu below a button).
export function bottomLeft(element) {
    const rect = element.getBoundingClientRect();
    return { x: rect.left, y: rect.bottom };
}
