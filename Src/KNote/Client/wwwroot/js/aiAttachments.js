// The files of a message of the AI assistant (Pages/AIAssistant/AIAssistant.razor): pasted into the message box, dropped on it or
// chosen with the Attach button, each one is read as base64 and handed to the page (dotNetRef.OnAttachment). The page attaches it
// when the message is sent. A paste without files is text, and goes on as always.
export function init(element, dotNetRef, maxBytes) {
    const hasFiles = e => Array.from(e.dataTransfer?.types || []).includes('Files');

    const toBase64 = file => new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.substring(reader.result.indexOf(',') + 1));
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });

    async function add(files) {
        for (const file of Array.from(files || [])) {
            if (file.size > maxBytes) {
                await dotNetRef.invokeMethodAsync('OnAttachmentTooBig', file.name || 'file');
                continue;
            }
            await dotNetRef.invokeMethodAsync('OnAttachment', file.name || '', file.type || '', await toBase64(file));
        }
    }

    function onPaste(e) {
        if (!e.clipboardData?.files?.length)
            return;
        e.preventDefault();
        add(e.clipboardData.files);
    }

    function onDragOver(e) {
        if (!hasFiles(e))
            return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
        element.classList.add('ai-prompt-dropping');
    }

    function onDragLeave(e) {
        if (!element.contains(e.relatedTarget))
            element.classList.remove('ai-prompt-dropping');
    }

    function onDrop(e) {
        element.classList.remove('ai-prompt-dropping');
        if (!hasFiles(e))
            return;
        e.preventDefault();
        add(e.dataTransfer.files);
    }

    element.addEventListener('paste', onPaste);
    element.addEventListener('dragover', onDragOver);
    element.addEventListener('dragleave', onDragLeave);
    element.addEventListener('drop', onDrop);

    return {
        pick: () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.multiple = true;
            input.onchange = () => add(input.files);
            input.click();
        },
        dispose: () => {
            element.removeEventListener('paste', onPaste);
            element.removeEventListener('dragover', onDragOver);
            element.removeEventListener('dragleave', onDragLeave);
            element.removeEventListener('drop', onDrop);
        }
    };
}
