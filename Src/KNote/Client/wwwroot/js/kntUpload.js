// KNote Atlas, stage 4 (D6): sends the file chosen in an <input type="file"> to the server as a form (multipart), straight
// from the disk: the page never holds it (a resource can be of 200 MB). Its progress goes to .NET (OnUploadProgress), at
// most once per percent, and it can be canceled. One upload at a time (the dialog that sends it is modal).

let current = null;

export function upload(input, url, token, fields, dotNetRef) {
    const file = input && input.files && input.files[0];
    if (!file)
        return Promise.resolve({ status: 0, body: null, error: "No file chosen.", canceled: false });

    const form = new FormData();
    for (const [name, value] of Object.entries(fields || {}))
        form.append(name, value == null ? "" : String(value));
    form.append("file", file, file.name);

    const xhr = new XMLHttpRequest();
    let lastPercent = -1;
    const done = new Promise(resolve => {
        xhr.upload.onprogress = e => {
            if (!e.lengthComputable || !dotNetRef)
                return;
            const percent = Math.floor(e.loaded * 100 / e.total);
            if (percent === lastPercent)
                return;
            lastPercent = percent;
            dotNetRef.invokeMethodAsync("OnUploadProgress", e.loaded, e.total).catch(() => { });
        };
        xhr.onload = () => resolve({ status: xhr.status, body: xhr.responseText, error: null, canceled: false });
        xhr.onerror = () => resolve({ status: 0, body: null, error: "The web server could not be reached.", canceled: false });
        xhr.onabort = () => resolve({ status: 0, body: null, error: null, canceled: true });
        xhr.onloadend = () => { if (current === xhr) current = null; };
    });

    xhr.open("POST", url);
    if (token)
        xhr.setRequestHeader("Authorization", "Bearer " + token);
    current = xhr;
    xhr.send(form);
    return done;
}

export function cancel() {
    if (current)
        current.abort();
}
