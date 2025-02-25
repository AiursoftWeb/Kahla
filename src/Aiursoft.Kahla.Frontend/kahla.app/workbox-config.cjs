module.exports = {
    globDirectory: './dist/ng/browser',
    globPatterns: ['**/*.{png,js,ico,html,json,woff2,css}'],
    swDest: 'dist/ng/browser/sw.js',
    ignoreURLParametersMatching: [/^utm_/, /^fbclid$/],
    importScripts: ['./sw_notifications.js'],
    sourcemap: false,
    cleanupOutdatedCaches: true,
    navigateFallback: '/index.html',
    navigateFallbackDenylist: [/^\/api\//, /^\/assets\//],
};
