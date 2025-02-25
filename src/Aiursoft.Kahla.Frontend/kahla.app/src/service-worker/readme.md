# About service worker

Kahla's service worker provide caching and push notification service for Kahla. Typically, it consists of two part:

- `sw_notification.js`: The service worker file for push notification. Build with `esbuild`.
- `sw.js`: The service worker for caching and importing `sw_notification.js`. Build with `workbox-build`.

In development server, the `sw.js` will not be built. Instead, a dummy service worker which only imports `sw_notification.js` will be used (located at `public/sw.js`).

Note: `sw_notification.js` doesn't support hot-reload. You need to re-run the development server to apply changes.
