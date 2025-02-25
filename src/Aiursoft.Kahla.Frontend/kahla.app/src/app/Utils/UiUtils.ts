export function getRandomColor(darkColor: boolean): string {
    let r = Math.floor(Math.random() * 128);
    let g = Math.floor(Math.random() * 128);
    let b = Math.floor(Math.random() * 128);
    if (!darkColor) {
        r = Math.max(r + 127, 200);
        g = Math.max(g + 127, 200);
        b = Math.max(b + 127, 200);
    }
    return `rgb(${r}, ${g}, ${b})`;
}
