import { KahlaEvent } from '../app/Models/Events/KahlaEvent';
import {
    eventNotificationDescription,
    eventNotificationUrl,
} from '../app/Models/Events/EventUtils';
import type { ServiceWorkerIpcMessage } from '../app/Models/ServiceWorker/ServiceWorkerIpc';
import { KahlaEventType } from '../app/Models/Events/EventType';

const sw = self as unknown as ServiceWorkerGlobalScope;

async function getWindowClients() {
    const clients = (await sw.clients.matchAll()).filter(
        t => t.type === 'window'
    ) as WindowClient[];

    return clients;
}

export interface NotificationContext {
    preferredUrl: string;
}

sw.addEventListener('notificationclick', event =>
    event.waitUntil(
        (async () => {
            const data = event.notification.data as NotificationContext;
            const clients = await getWindowClients();
            if (clients.length) {
                clients[0].postMessage({
                    type: 'navigate',
                    url: data.preferredUrl,
                } satisfies ServiceWorkerIpcMessage);
                await clients[0].focus();
            } else {
                await sw.clients.openWindow(data.preferredUrl);
            }
            event.notification.close();
        })()
    )
);

sw.addEventListener('push', event =>
    event.waitUntil(
        (async () => {
            const data = event.data!.json() as KahlaEvent;
            const [title, description] = eventNotificationDescription(data);
            const url = eventNotificationUrl(data);
            if (data.type === KahlaEventType.NewMessage) {
                // new message
                const bypassNotification = (await getWindowClients())
                    .filter(t => t.focused)
                    .some(t => t.url.startsWith(url));

                if (bypassNotification) return;
            }

            await sw.registration.showNotification(title, {
                body: description,
                tag: url,
                data: {
                    preferredUrl: url,
                },
            });
        })()
    )
);
