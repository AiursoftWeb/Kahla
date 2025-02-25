export function mapDeviceName(ua: string): [string, string] {
    let os = 'Unknown OS';
    // OS
    if (ua.includes('Win')) {
        os = 'Windows';
    } else if (ua.includes('Android')) {
        os = 'Android';
    } else if (ua.includes('Linux')) {
        os = 'Linux';
    } else if (ua.includes('iPhone') || ua.includes('iPad')) {
        os = 'iOS';
    } else if (ua.includes('Mac')) {
        os = 'macOS';
    }
    let browser = 'Unknown browser';
    // Browser Name
    if (ua.includes('Firefox') && !ua.includes('Seamonkey')) {
        browser = 'Firefox';
    } else if (ua.includes('Seamonkey')) {
        browser = 'Seamonkey';
    } else if (ua.includes('Edge')) {
        browser = 'Microsoft Edge';
    } else if (ua.includes('Edg')) {
        browser = 'Edge Chromium';
    } else if (ua.includes('Chrome') && !ua.includes('Chromium')) {
        browser = 'Chrome';
    } else if (ua.includes('Chromium')) {
        browser = 'Chromium';
    } else if (ua.includes('Safari') && (!ua.includes('Chrome') || !ua.includes('Chromium'))) {
        browser = 'Safari';
    } else if (ua.includes('Opera') || ua.includes('OPR')) {
        browser = 'Opera';
    } else if (/MSIE|Trident/.exec(ua)) {
        browser = 'Internet Explorer';
    }

    return [os, browser];
}
