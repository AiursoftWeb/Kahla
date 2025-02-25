export function scrollBottom(smooth: boolean, onlyIfReadPixels?: number): boolean {
    if (onlyIfReadPixels != undefined) {
        const h = document.documentElement.scrollHeight;
        const y = window.scrollY;
        if (y + window.innerHeight < h - onlyIfReadPixels) {
            return false;
        }
    }
    const h = document.documentElement.scrollHeight;
    if (smooth) {
        window.scroll({ top: h, behavior: 'smooth' });
    } else {
        window.scroll(0, h);
    }
    return true;
}

export function scrollTop(smooth: boolean): void {
    if (smooth) {
        window.scroll({ top: 0, behavior: 'smooth' });
    } else {
        window.scroll(0, 0);
    }
}
