// Images in the chat room page (Pages/ChatRoom/ChatRoom.razor): pasted into the message box or dropped on the
// conversation, each image is read as base64 and handed to the page (dotNetRef.OnImage), which sends it. A dropped file
// that isn't an image is refused there too (OnNotAnImage), never opened by the browser. Clicking an image of a message
// opens it in another tab. An image of a message makes it taller once it loads, after the page has scrolled to the end:
// the conversation goes to its end again, so the image is seen whole.
export function init(element, dotNetRef, maxBytes) {
    const types = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'];

    const imagesOf = list => Array.from(list || []).filter(f => types.includes(f.type));
    const hasFiles = e => Array.from(e.dataTransfer?.types || []).includes('Files');

    const toBase64 = file => new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.substring(reader.result.indexOf(',') + 1));
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });

    async function send(files) {
        for (const file of files) {
            if (file.size > maxBytes) {
                await dotNetRef.invokeMethodAsync('OnImageTooBig', file.name || 'image');
                continue;
            }
            await dotNetRef.invokeMethodAsync('OnImage', file.name || '', file.type, await toBase64(file));
        }
    }

    function onPaste(e) {
        const files = imagesOf(e.clipboardData?.files);
        if (files.length === 0)
            return;
        e.preventDefault();
        send(files);
    }

    function onDragOver(e) {
        if (!hasFiles(e))
            return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
        element.classList.add('chat-room-dropping');
    }

    function onDragLeave(e) {
        if (!element.contains(e.relatedTarget))
            element.classList.remove('chat-room-dropping');
    }

    function onDrop(e) {
        element.classList.remove('chat-room-dropping');
        if (!hasFiles(e))
            return;
        e.preventDefault();
        const files = imagesOf(e.dataTransfer.files);
        if (files.length === 0)
            dotNetRef.invokeMethodAsync('OnNotAnImage');
        else
            send(files);
    }

    function onImageLoaded(e) {
        if (e.target.tagName !== 'IMG' || !e.target.closest('.chat-room-message-text'))
            return;
        const messages = element.querySelector('.rz-chat-messages');
        if (messages)
            messages.scrollTop = messages.scrollHeight;
    }

    function onClick(e) {
        const image = e.target.closest('.chat-room-message-text img');
        if (image)
            window.open(image.src, '_blank', 'noopener');
    }

    element.addEventListener('paste', onPaste);
    element.addEventListener('dragover', onDragOver);
    element.addEventListener('dragleave', onDragLeave);
    element.addEventListener('drop', onDrop);
    element.addEventListener('click', onClick);
    // 'load' doesn't bubble: caught on its way down.
    element.addEventListener('load', onImageLoaded, true);

    return {
        dispose: () => {
            element.removeEventListener('paste', onPaste);
            element.removeEventListener('dragover', onDragOver);
            element.removeEventListener('dragleave', onDragLeave);
            element.removeEventListener('drop', onDrop);
            element.removeEventListener('click', onClick);
            element.removeEventListener('load', onImageLoaded, true);
        }
    };
}
