// Windows of the app (AppStoreService/KntWindows): a note is viewed or edited in a window of its own, one per note
// and use (the AI assistant and the chat room, one each), and the windows of the app tell each other when a note is
// saved (BroadcastChannel, same origin).

const channel = new BroadcastChannel('knote-notes');

// The new windows cascade from this one: each one a step further down and right, back to the first step after a
// few, and always inside the screen.
const cascadeStep = 32;
const cascadeSteps = 8;
let cascade = 0;

// Opens url in the window called name, or brings that window to the front as it is (an edit in progress is kept,
// and it is not moved). False if the browser blocked it (it must be called right after a click or a key).
export function openWindow(url, name, width, height) {
    width = Math.min(width, screen.availWidth - 40);
    height = Math.min(height, screen.availHeight - 40);
    const offset = cascadeStep * (cascade % cascadeSteps + 1);
    const left = Math.min(window.screenX + offset, (screen.availLeft ?? 0) + screen.availWidth - width);
    const top = Math.min(window.screenY + offset, (screen.availTop ?? 0) + screen.availHeight - height);
    const win = window.open('', name, `popup,width=${width},height=${height},left=${left},top=${top}`);
    if (!win)
        return false;
    if (win.location.href === 'about:blank') {
        cascade++;
        win.location.href = url;
    }
    win.focus();
    return true;
}

// This window becomes the one called name, at url (without loading it again).
export function renameWindow(name, url) {
    window.name = name;
    history.replaceState(history.state, '', url);
}

// The browsers only close the windows opened by a script: otherwise (e.g. the address pasted in a tab) the app
// goes on at fallbackUrl.
export function closeWindow(fallbackUrl) {
    window.close();
    setTimeout(() => {
        if (!window.closed)
            location.href = fallbackUrl;
    }, 200);
}

// Asks before the window is closed or reloaded while dotNet.HasUnsavedChanges() says so.
export function watchUnsavedChanges(dotNet) {
    const onBeforeUnload = e => {
        if (dotNet.invokeMethod('HasUnsavedChanges')) {
            e.preventDefault();
            e.returnValue = '';
        }
    };
    window.addEventListener('beforeunload', onBeforeUnload);
    return { dispose: () => window.removeEventListener('beforeunload', onBeforeUnload) };
}

export function notifyNoteSaved(noteId) {
    channel.postMessage({ noteId });
}

// dotNet.OnNoteSaved(noteId) when another window saves a note (a window doesn't hear its own messages).
export function listenNoteSaved(dotNet) {
    const onMessage = e => dotNet.invokeMethodAsync('OnNoteSaved', e.data.noteId);
    channel.addEventListener('message', onMessage);
    return { dispose: () => channel.removeEventListener('message', onMessage) };
}
