const CACHE = 'v6';
const UPDATE_REQUIRED = '__Update_Required__';
const UPDATE_COMPLETED = '__Update_Completed__';
const self_scope = self.registration.scope;

//region cache-control

function installCache() {
    return caches.open(CACHE).then(function (cache) {
        let requests = [
            '/index.html',
            '/main.js',
            '/manifest.json',
            '/polyfills.js',
            '/styles.css',
            '/favicon.ico',
            '/favicon_notify.ico',
            '/media/fa-solid-900.woff2',
            '/media/fa-regular-400.woff2',
            '/media/fa-brands-400.woff2',
            '/assets/144x144.png'
        ].map(t => new Request(t, {cache: 'no-cache'}))
        return cache.addAll(requests);
    });
}

self.addEventListener('install', function (event) {
    event.waitUntil(installCache());
});

self.addEventListener('fetch', function (event) {
    // bypass upload request
    if (event.request.method !== 'GET' || !event.request.url.startsWith(self_scope)) {
        return;
    }

    event.respondWith(
        caches.match(event.request).then(function (response) {
            return response || fetch(event.request);
        })
    );
    event.waitUntil(
        caches.match(event.request).then(function (response) {
            if (response) {
                caches.open(CACHE).then(function (cache) {
                    fetch(event.request).then(function (resp) {
                        cache.put(event.request, resp);
                    });
                });
            }
        })
    );
});

self.addEventListener('activate', function (event) {
    const cacheKeeplist = [CACHE];
    event.waitUntil(
        self.clients.claim(),
        caches.keys().then(function (keyList) {
            return Promise.all(keyList.map(function (key) {
                if (cacheKeeplist.indexOf(key) === -1) {
                    return caches.delete(key);
                }
            }));
        })
    );
});

self.addEventListener('message', function (event) {
    if (event.data === UPDATE_REQUIRED) {
        console.log('Force Cache Upgrade triggered.');
        caches.delete(CACHE).then(() => {
            return installCache();
        }).then(() => {
            self.clients.matchAll().then(clients => {
                console.log(clients);
                for (let i = 0; i < clients.length; i++) {
                    clients[i].postMessage(UPDATE_COMPLETED);
                }
            })
        })
    }
});

//endregion

//region Notifications

self.addEventListener('notificationclick', function (event) {
    const data = event.notification.data;
    event.waitUntil(
        self.clients.matchAll().then(function (clientList) {
            if (clientList.length > 0) {
                return clientList[0].focus();
            } else {
                if (data && data.type === 0 && data.message.conversationID !== -1) {
                    return self.clients.openWindow(`/talking/${data.message.conversationId}`);
                } else {
                    return self.clients.openWindow('/');
                }
            }
        })
    );
    event.notification.close();
});

self.addEventListener('push', function (event) {
    if (!event.data) {
        return;
    }
    let data = event.data.json();
    const pushTitle = 'Aiursoft Push System';
    const imageLink = `https://probe.aiursoft.com/Download/Open/${encodeURIComponent(data.message.sender.iconFilePath).replace(/%2F/g, '/')}`;

    if (data.type == 0 && !data.muted) {
        // new message
        const title = (data.mentioned ? '[Mentioned you] ' : '') + data.message.sender.nickName;
        let message = data.message.content;

        if (message.startsWith('[img]')) {
            message = 'Photo';
        } else if (message.startsWith('[video]')) {
            message = 'Video';
        } else if (message.startsWith('[file]')) {
            message = 'File';
        } else if (message.startsWith('[audio]')) {
            message = 'Audio';
        } else if (message.startsWith('[group]')) {
            message = 'Group Invitation'
        } else if (message.startsWith('[user]')) {
            message = 'Contact card';
        }

        let showNotification = true;
        event.waitUntil(
            self.clients.matchAll().then(function (clientList) {
                clientList.forEach(function (client) {
                    const URLArray = client.url.split('/');
                    let URLId = -1;
                    let talkingPage = false;
                    if (URLArray.length > 4) {
                        URLId = parseInt(URLArray[4]);
                        if (URLArray[3] == 'talking') {
                            talkingPage = true;
                        }
                    }
                    if (!isNaN(URLId) && URLId == data.message.conversationId && client.focused && talkingPage) {
                        showNotification = false;
                    }
                });

                if (showNotification) {
                    return self.registration.showNotification(title, {
                        body: message,
                        icon: imageLink,
                        renotify: true,
                        tag: data.message.conversationId.toString(),
                        data: data
                    });
                }
            })
        );
    } else if (data.type == 1) {
        // new friend request
        self.registration.showNotification(pushTitle, {
            body: 'You have got a new friend request!',
            icon: imageLink,
            renotify: true,
            tag: -1,
            data: data
        });
    } else if (data.type == 2) {
        // were deleted event
        self.registration.showNotification(pushTitle, {
            body: 'You were deleted by one of your friends from his friend list.',
            icon: imageLink,
            renotify: true,
            tag: -1,
            data: data
        });
    } else if (data.type == 3) {
        // friend accepted event
        self.registration.showNotification(pushTitle, {
            body: 'Your friend request was accepted!',
            icon: imageLink,
            renotify: true,
            tag: -1,
            data: data
        });
    }
});

//endregion Notifications
