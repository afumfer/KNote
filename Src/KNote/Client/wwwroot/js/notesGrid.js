// DOM helpers for Pages/Notes/NotesGrid (a RadzenDataGrid with virtualization). host is the element that wraps
// the grid (with the total of notes in data-count); rows carry the id of their note in data-note-id (RowRender)
// and their position in the whole list in data-knt-index (Radzen's aria-rowindex goes wrong when the grid is
// drawn again by its parent).

function scrollArea(host) {
    return host.querySelector('.rz-data-grid-data');
}

// The current row, like the current row of a desktop grid: selected with a click or moved with the keyboard. Radzen's own arrow navigation counts the
// rows drawn, not the rows of the list, so with virtualization it loses its place: these keys are handled here
// and don't reach Radzen. The current row is marked with data-knt-current; actions go to dotNet.OnKeyAction.
export function initialize(host, dotNet) {
    let current = -1;

    const rows = () => [...host.querySelectorAll('tr.rz-data-row[data-knt-index]')];
    const indexOf = row => Number(row.dataset.kntIndex);
    const rowAt = index => host.querySelector(`tr.rz-data-row[data-knt-index="${index}"]`);
    const count = () => Number(host.dataset.count || 0);
    const metrics = () => {
        const area = scrollArea(host);
        return {
            area,
            rowHeight: rows()[0]?.offsetHeight || 25,
            header: area.querySelector('thead')?.offsetHeight ?? 0
        };
    };

    // Rows are drawn again while scrolling (and reused for other notes): the mark follows the index. Coming back
    // to the list, the note opened last is the current one. .NET hears of the note of the current row
    // (OnCurrentChanged) when it changes.
    let notifiedNoteId = null;
    const sync = () => {
        if (current < 0) {
            const last = host.querySelector('tr.rz-data-row[data-last-opened][data-knt-index]');
            if (last)
                current = indexOf(last);
        }
        host.querySelectorAll('tr[data-knt-current]').forEach(row => {
            if (indexOf(row) !== current)
                row.removeAttribute('data-knt-current');
        });
        const row = rowAt(current);
        if (row && !row.hasAttribute('data-knt-current'))
            row.setAttribute('data-knt-current', '');
        if (row && row.dataset.noteId !== notifiedNoteId) {
            notifiedNoteId = row.dataset.noteId;
            dotNet.invokeMethodAsync('OnCurrentChanged', notifiedNoteId);
        }
    };

    const visibleRows = () => {
        const { area, header } = metrics();
        const box = area.getBoundingClientRect();
        return rows().filter(row => {
            const r = row.getBoundingClientRect();
            return r.top >= box.top + header - 1 && r.bottom <= box.bottom + 1;
        });
    };

    // Where the keyboard starts: the note opened last if it is in view, or the first row in view.
    const startIndex = () => {
        const visible = visibleRows();
        const last = visible.find(row => row.hasAttribute('data-last-opened'));
        return last ? indexOf(last) : (visible.length ? indexOf(visible[0]) : 0);
    };

    const moveTo = index => {
        if (count() === 0)
            return;
        current = Math.max(0, Math.min(count() - 1, index));
        const { area, rowHeight, header } = metrics();
        const top = header + current * rowHeight;
        if (top < area.scrollTop + header)
            area.scrollTop = top - header;
        else if (top + rowHeight > area.scrollTop + area.clientHeight)
            area.scrollTop = top + rowHeight - area.clientHeight;
        sync();
    };

    // On the current row, brought into view first (if it was scrolled away it is drawn some frames later).
    const act = action => {
        moveTo(current < 0 ? startIndex() : current);
        let frames = 30;
        const run = () => {
            const row = rowAt(current);
            if (row)
                dotNet.invokeMethodAsync('OnKeyAction', action, row.dataset.noteId);
            else if (frames-- > 0)
                requestAnimationFrame(run);
        };
        run();
    };

    const onKeyDown = e => {
        const area = scrollArea(host);
        if (!area || !area.contains(e.target))
            return;

        const from = current < 0 ? startIndex() : current;
        const step = current < 0 ? 0 : 1;
        const page = Math.max(1, Math.floor((area.clientHeight - metrics().header) / metrics().rowHeight) - 1);
        switch (e.key) {
            case 'ArrowDown': moveTo(from + step); break;
            case 'ArrowUp': moveTo(from - step); break;
            case 'PageDown': moveTo(from + page); break;
            case 'PageUp': moveTo(from - page); break;
            case 'Home': moveTo(0); break;
            case 'End': moveTo(count() - 1); break;
            case 'Enter': act('view'); break;
            case 'Delete': act('delete'); break;
            case 'ContextMenu': act('menu'); break;
            case 'F10':
                if (!e.shiftKey)
                    return;
                act('menu');
                break;
            default:
                return;
        }
        e.preventDefault();
        e.stopPropagation();
    };

    // A row pressed with the mouse (also with the right button) becomes the current one.
    const onMouseDown = e => {
        const row = e.target.closest('tr.rz-data-row[data-knt-index]');
        if (row) {
            current = indexOf(row);
            sync();
        }
    };

    const observer = new MutationObserver(sync);
    observer.observe(host, { subtree: true, childList: true, attributes: true, attributeFilter: ['data-knt-index'] });
    host.addEventListener('keydown', onKeyDown, true);
    host.addEventListener('mousedown', onMouseDown, true);

    return {
        dispose: () => {
            observer.disconnect();
            host.removeEventListener('keydown', onKeyDown, true);
            host.removeEventListener('mousedown', onMouseDown, true);
        }
    };
}

export function getScrollTop(host) {
    return scrollArea(host)?.scrollTop ?? 0;
}

export function setScrollTop(host, value) {
    const area = scrollArea(host);
    if (area)
        area.scrollTop = value;
}

// Scrolls to a position kept from before, once the grid has drawn its whole height (the space of the rows
// not loaded yet), which happens some frames after the first rows arrive.
export function restoreScrollTop(host, value) {
    let frames = 60;
    const restore = () => {
        const area = scrollArea(host);
        if (area && area.scrollHeight - area.clientHeight >= value)
            area.scrollTop = value;
        else if (frames-- > 0)
            requestAnimationFrame(restore);
    };
    restore();
}

// Point below a row, at its topic, to open its menu there.
export function getActionsAnchor(host, noteId) {
    const cell = host.querySelector(`tr[data-note-id="${noteId}"] td:nth-child(2)`);
    if (!cell)
        return null;
    const rect = cell.getBoundingClientRect();
    return { x: rect.left + 16, y: rect.bottom };
}
