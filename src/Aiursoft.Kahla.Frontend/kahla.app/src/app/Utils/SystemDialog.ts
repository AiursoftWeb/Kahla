export const imageFileTypes = ['image/png', 'image/jpeg', 'image/webp', 'image/gif'];

export function selectFiles(
    multiple = false,
    accept: string[] | '*/*' = imageFileTypes
): Promise<File[]> {
    return new Promise((resolve, reject) => {
        const input = document.createElement('input');
        input.type = 'file';
        // The user can still select incorrect files even if the accept attribute is set
        input.accept = accept === '*/*' ? accept : accept.join(', ');
        input.multiple = multiple;
        input.onchange = e => {
            const files = Array.from((e.target as HTMLInputElement).files ?? []).filter(
                t => accept === '*/*' || accept.includes(t.type)
            );
            if (files.length > 0) {
                resolve(files);
            } else {
                reject(new Error('No files selected'));
            }
        };
        input.click();
    });
}

export function selectDirectory(accept: string[] | '*/*' = imageFileTypes): Promise<File[]> {
    return new Promise((resolve, reject) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.webkitdirectory = true;
        input.onchange = e => {
            let files = Array.from((e.target as HTMLInputElement).files ?? []);
            if (accept !== '*/*') {
                files = files.filter(t => accept.includes(t.type));
            }
            if (files.length > 0) {
                resolve(Array.from(files));
            } else {
                reject(new Error('No files selected'));
            }
        };
        input.click();
    });
}
