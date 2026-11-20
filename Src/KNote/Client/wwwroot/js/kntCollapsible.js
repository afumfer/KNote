// Shared/KntCollapsible: content shown up to a number of lines, with "Show more" only when it is longer. The content is HTML of any
// kind (a note's description: headings, tables, images, Mermaid diagrams), so its lines are not counted: the limit is that many
// times its line height from where its first line of text starts (after the top margin of its first paragraph or heading), and
// the content is measured again whenever its size changes (an image that loads, a diagram drawn, a narrower window). body is the
// box that is cut; content, its whole content inside it.

function lineHeight(element) {
    const style = getComputedStyle(element);
    const height = parseFloat(style.lineHeight);
    // "normal": about 1.5 times the font in the themes of the app.
    return Number.isNaN(height) ? parseFloat(style.fontSize) * 1.5 : height;
}

// Where the first line of text starts, from the top of the content (0 without text: an image, a diagram).
function firstLineTop(content) {
    const walker = document.createTreeWalker(content, NodeFilter.SHOW_TEXT, {
        acceptNode: node => node.textContent.trim() ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP
    });
    const text = walker.nextNode();
    if (!text)
        return 0;
    const range = document.createRange();
    range.selectNodeContents(text);
    const top = range.getBoundingClientRect().top - content.getBoundingClientRect().top;
    return Math.max(0, top);
}

// The element that scrolls the page (in the app, the layout's body, not the window).
function scrollParent(element) {
    for (let parent = element.parentElement; parent; parent = parent.parentElement) {
        const overflow = getComputedStyle(parent).overflowY;
        if ((overflow === 'auto' || overflow === 'scroll') && parent.scrollHeight > parent.clientHeight)
            return parent;
    }
    return null;
}

export function initialize(body, content, lines, dotNet) {
    let overflows = null;

    const measure = () => {
        const line = lineHeight(content);
        const limit = Math.round(firstLineTop(content) + line * lines);
        body.style.setProperty('--knt-collapsible-max', `${limit}px`);
        // Longer: more than those lines and the bottom margin of its last paragraph (less than a line).
        const longer = content.offsetHeight > limit + line;
        if (longer !== overflows) {
            overflows = longer;
            dotNet.invokeMethodAsync('SetOverflows', longer);
        }
    };

    const observer = new ResizeObserver(measure);
    observer.observe(content);
    measure();

    return {
        // Shown less: if its top was scrolled out of sight (above the page's scrolling area, under its fixed header), back to it,
        // so the reader is not left further down the page.
        // Its card, when it is in one, so that what it belongs to (the note's topic) is seen too.
        showTop: element => {
            const target = element.closest('.rz-card') ?? element;
            const parent = scrollParent(target);
            const top = parent ? parent.getBoundingClientRect().top : 0;
            if (target.getBoundingClientRect().top < top)
                target.scrollIntoView({ block: 'start' });
        },
        dispose: () => observer.disconnect()
    };
}
