export function urlBase64ToUint8Array(base64String: string): Uint8Array {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

export function checkSingleEmoji(text: string): boolean {
    if (text.length > 2) {
        return false;
    }
    const regex =
        /(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])/g;
    return regex.test(text);
}

export function humanReadableBytes(bytes: number) {
    const unit = ['', 'K', 'M', 'G', 'T'];
    let current = 0;
    while (current < unit.length && bytes > 1024) {
        bytes /= 1024;
        current++;
    }

    return `${bytes.toFixed(2)} ${unit[current]}B`;
}

/**
 * Truncates a UTF-8 encoded string to a specified maximum number of bytes.
 *
 * @param str - The input string to be truncated.
 * @param maxBytes - The maximum number of bytes allowed in the truncated string.
 * @param addEllipsisIfOverflow - A boolean flag indicating whether to add an ellipsis ('...') if the string is truncated.
 *                                Warning: the ellipsis will NOT count in the byte limit.
 *                                So the actual byte length of the truncated string may exceed the limit by 3 bytes if enabled.
 * @returns The truncated string, optionally with an ellipsis if it was truncated.
 */
export function truncateUTF8Bytes(
    str: string,
    maxBytes: number,
    addEllipsisIfOverflow = false
): string {
    let bytes = 0;
    let i = 0;
    for (i = 0; i < str.length; i++) {
        const charCode = str.charCodeAt(i);
        if (charCode <= 0x7f) {
            bytes += 1;
        } else if (charCode <= 0x7ff) {
            bytes += 2;
        } else if (charCode <= 0xffff) {
            bytes += 3;
        } else {
            bytes += 4;
        }
        if (bytes > maxBytes) {
            break;
        }
    }
    return str.slice(0, i) + (addEllipsisIfOverflow && i < str.length ? '...' : '');
}
