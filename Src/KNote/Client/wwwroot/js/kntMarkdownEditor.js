// Editor of a note's description (Shared/KntMarkdownEditor.razor): its Markdown in a textarea, edited by the browser.
// .NET gets the text now and then, not at each key (each key would cost as much as the note is long): at the first change
// (the note is modified then, and the unsaved changes check asks .NET without waiting), after a pause of `delay` ms in
// the typing, when the textarea loses the focus (with where the cursor was, OnLeave) and when .NET asks for it (flush,
// before saving: the next change is the first one again). The toolbar's commands change the text with execCommand('insertText'), so the browser's own
// undo (Ctrl+Z, the Undo button) undoes them too.
export function init(textarea, dotNetRef, text, delay, selectionStart, selectionEnd, scrollTop, focus) {
    let sent = '';
    // .NET knows the text changed since it was given, or since it was last asked for.
    let changed = false;
    let timer = null;

    function setText(value) {
        textarea.value = value;
        // As the browser keeps it (its line breaks are LF), so the text given isn't sent back as a change.
        sent = textarea.value;
        changed = false;
    }

    // The text if it changed since it was last sent; null if not.
    function take() {
        clearTimeout(timer);
        timer = null;
        const value = textarea.value;
        if (value === sent)
            return null;
        sent = value;
        return value;
    }

    function send() {
        const value = take();
        if (value !== null)
            dotNetRef.invokeMethodAsync('OnTextChanged', value).catch(() => { });
    }

    function onInput() {
        clearTimeout(timer);
        if (!changed) {
            changed = true;
            send();
        } else {
            timer = setTimeout(send, delay);
        }
    }

    function onBlur() {
        dotNetRef.invokeMethodAsync('OnLeave', take(), textarea.selectionStart, textarea.selectionEnd, textarea.scrollTop)
            // The editor may be going away (another tab of the note shown): nothing left to tell it.
            .catch(() => { });
    }

    // Replaces [start, end) with `text` and selects [selStart, selEnd), as one step of the browser's undo.
    function replace(start, end, text, selStart, selEnd) {
        textarea.focus();
        textarea.setSelectionRange(start, end);
        const done = document.activeElement === textarea &&
            document.execCommand(text === '' ? 'delete' : 'insertText', false, text);
        if (!done) {
            textarea.setRangeText(text, start, end, 'end');
            textarea.dispatchEvent(new Event('input', { bubbles: true }));
        }
        textarea.setSelectionRange(selStart, selEnd);
    }

    // **bold**, *italic*, ~~strikethrough~~, `code`: on the selection, or off if it already has it.
    function toggleInline(mark) {
        const value = textarea.value, start = textarea.selectionStart, end = textarea.selectionEnd;
        const selected = value.slice(start, end), n = mark.length;
        // An italic mark is a single *: not the side of a bold one (***both*** is both).
        const isMark = (side, atEnd) => {
            if (!(atEnd ? side.endsWith(mark) : side.startsWith(mark)))
                return false;
            if (mark !== '*')
                return true;
            const two = atEnd ? side.endsWith('**') : side.startsWith('**');
            const three = atEnd ? side.endsWith('***') : side.startsWith('***');
            return !two || three;
        };
        if (selected.length >= 2 * n && isMark(selected, false) && isMark(selected, true)) {
            const inner = selected.slice(n, selected.length - n);
            replace(start, end, inner, start, start + inner.length);
        } else if (isMark(value.slice(0, start), true) && isMark(value.slice(end), false)) {
            replace(start - n, end + n, selected, start - n, end - n);
        } else {
            // Spaces at the sides of the selection (a double click takes the one after a word) stay outside.
            const core = selected.trim();
            const lead = selected.slice(0, selected.indexOf(core));
            const trail = selected.slice(lead.length + core.length);
            const at = start + lead.length + n;
            replace(start, end, lead + mark + core + mark + trail, at, at + core.length);
        }
    }

    // The whole lines the selection touches (one that ends at the start of a line doesn't take that line).
    function lineRange() {
        const value = textarea.value, start = textarea.selectionStart;
        let end = textarea.selectionEnd;
        if (end > start && value[end - 1] === '\n')
            end--;
        const from = start === 0 ? 0 : value.lastIndexOf('\n', start - 1) + 1;
        const next = value.indexOf('\n', end);
        return [from, next < 0 ? value.length : next];
    }

    // Changes each line the selection touches (the empty ones stay as they are, unless it is the only line).
    function changeLines(change) {
        const [from, to] = lineRange();
        const old = textarea.value.slice(from, to);
        const lines = old.split('\n');
        const used = lines.filter(line => line.trim() !== '');
        const text = change(lines, used.length > 0 ? used : lines).join('\n');
        const caret = textarea.selectionStart === textarea.selectionEnd && lines.length === 1;
        if (caret) {
            const at = Math.max(from, textarea.selectionStart + text.length - old.length);
            replace(from, to, text, at, at);
        } else {
            replace(from, to, text, from, from + text.length);
        }
    }

    const listMarker = /^([-*+] \[[ xX]\] |[-*+] |\d+[.)] )/;
    const prefixes = {
        quote: { has: /^> ?/, add: () => '> ' },
        bullet: { has: /^[-*+] (?!\[[ xX]\] )/, add: () => '- ' },
        numbered: { has: /^\d+[.)] /, add: i => `${i + 1}. ` },
        task: { has: /^[-*+] \[[ xX]\] /, add: () => '- [ ] ' },
    };

    // A quote or a list on the lines, or off if they all have it already. A list replaces another one.
    function toggleLines(kind) {
        const prefix = prefixes[kind];
        changeLines((lines, used) => {
            const split = line => { const indent = line.match(/^\s*/)[0]; return [indent, line.slice(indent.length)]; };
            const off = used.every(line => prefix.has.test(split(line)[1]));
            let index = 0;
            return lines.map(line => {
                if (!used.includes(line))
                    return line;
                const [indent, rest] = split(line);
                if (off)
                    return indent + rest.replace(prefix.has, '');
                const bare = kind === 'quote' ? rest : rest.replace(listMarker, '');
                return indent + prefix.add(index++) + bare;
            });
        });
    }

    // Headings of a level (1 to 6), or normal text (0).
    function heading(level) {
        changeLines((lines, used) => lines.map(line => {
            if (!used.includes(line))
                return line;
            const bare = line.replace(/^#{1,6}\s+/, '');
            return level > 0 ? '#'.repeat(level) + ' ' + bare : bare;
        }));
    }

    // A block (code, table, line, a resource) in its own paragraph, instead of the selection; selects [from, to) of it.
    function insertBlock(block, from = block.length, to = from) {
        const value = textarea.value, start = textarea.selectionStart, end = textarea.selectionEnd;
        const before = value.slice(0, start), after = value.slice(end);
        const lead = start === 0 ? '' : before.endsWith('\n\n') ? '' : before.endsWith('\n') ? '\n' : '\n\n';
        const trail = after === '' ? '' : after.startsWith('\n\n') ? '' : after.startsWith('\n') ? '\n' : '\n\n';
        const at = start + lead.length;
        replace(start, end, lead + block + trail, at + from, at + to);
    }

    function codeBlock() {
        const selected = textarea.value.slice(textarea.selectionStart, textarea.selectionEnd);
        insertBlock('```\n' + selected + '\n```', 4, 4 + selected.length);
    }

    function table() {
        const block = '| Column 1 | Column 2 | Column 3 |\n| --- | --- | --- |\n|  |  |  |\n|  |  |  |';
        insertBlock(block, 2, 2 + 'Column 1'.length);
    }

    // [text](url) or ![text](url): the selection is its text and the url is selected to be written, or, if the selection
    // is a url, the text is.
    function link(image) {
        const start = textarea.selectionStart, end = textarea.selectionEnd;
        const selected = textarea.value.slice(start, end).trim();
        const bang = image ? '!' : '';
        if (/^(https?:\/\/|mailto:)\S+$/.test(selected)) {
            const label = image ? 'alt text' : 'text';
            replace(start, end, `${bang}[${label}](${selected})`, start + bang.length + 1, start + bang.length + 1 + label.length);
        } else {
            const label = selected || (image ? 'alt text' : 'text');
            const at = start + bang.length + label.length + 3;
            replace(start, end, `${bang}[${label}](https://)`, at, at + 'https://'.length);
        }
    }

    function history(command) {
        textarea.focus();
        document.execCommand(command);
    }

    const commands = {
        undo: () => history('undo'),
        redo: () => history('redo'),
        heading: level => heading(level),
        bold: () => toggleInline('**'),
        italic: () => toggleInline('*'),
        strikethrough: () => toggleInline('~~'),
        code: () => toggleInline('`'),
        quote: () => toggleLines('quote'),
        codeBlock,
        bullet: () => toggleLines('bullet'),
        numbered: () => toggleLines('numbered'),
        task: () => toggleLines('task'),
        link: () => link(false),
        image: () => link(true),
        rule: () => insertBlock('---'),
        table,
    };

    const shortcuts = { b: 'bold', i: 'italic', k: 'link' };

    function onKeyDown(e) {
        if (!(e.ctrlKey || e.metaKey) || e.altKey || e.shiftKey)
            return;
        const command = shortcuts[e.key.toLowerCase()];
        if (!command)
            return;
        e.preventDefault();
        commands[command]();
    }

    setText(text);
    if (selectionStart !== null && selectionStart !== undefined) {
        const length = textarea.value.length;
        textarea.setSelectionRange(Math.min(selectionStart, length), Math.min(selectionEnd ?? selectionStart, length));
    } else {
        textarea.setSelectionRange(0, 0);
    }
    if (focus)
        textarea.focus({ preventScroll: true });
    textarea.scrollTop = scrollTop ?? 0;

    textarea.addEventListener('input', onInput);
    textarea.addEventListener('blur', onBlur);
    textarea.addEventListener('keydown', onKeyDown);

    return {
        setText,
        flush: () => {
            changed = false;
            return take();
        },
        command: (name, arg) => commands[name]?.(arg),
        dispose: () => {
            clearTimeout(timer);
            textarea.removeEventListener('input', onInput);
            textarea.removeEventListener('blur', onBlur);
            textarea.removeEventListener('keydown', onKeyDown);
        }
    };
}
