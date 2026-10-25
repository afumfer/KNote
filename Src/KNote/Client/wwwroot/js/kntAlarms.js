// The alarms panel (AppStoreService/AlarmNotices): the notices kept in the browser (localStorage) are shared by the
// tabs of the app, which hear the changes of the others (the storage event only reaches the other tabs), and a new
// notice is a notification of the system while the app is not on screen, if the user allowed it.

export function permission() {
    return 'Notification' in window ? Notification.permission : 'unsupported';
}

export async function requestPermission() {
    if (!('Notification' in window))
        return 'unsupported';
    return await Notification.requestPermission();
}

// False when it isn't shown (the app is on screen, or not allowed): the app shows it.
export function notify(title, body, tag) {
    if (!document.hidden || permission() !== 'granted')
        return false;
    const notification = new Notification(title, { body, tag });
    notification.onclick = () => {
        window.focus();
        notification.close();
    };
    return true;
}

// key changed in another tab (or the whole storage cleared): dotNet.OnStorageChanged().
export function watchStorage(dotNet, key) {
    const onStorage = e => {
        if (e.storageArea === localStorage && (e.key === key || e.key === null))
            dotNet.invokeMethodAsync('OnStorageChanged');
    };
    window.addEventListener('storage', onStorage);
    return { dispose: () => window.removeEventListener('storage', onStorage) };
}
