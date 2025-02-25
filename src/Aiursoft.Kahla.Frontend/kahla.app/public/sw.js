// This is a dummy service worker file which is intended to be replaced by the workbox-generated on production build.
// This file should only be loaded in local development server.

self.addEventListener('install', () => {
    console.log('Dummy service worker installed.');
});

self.addEventListener('activate', () => {
    console.log(
        'Dummy service worker activated. I will not do anything about caching :D\nYou should only see this in development server.'
    );
});

importScripts('./sw_notifications.js');
