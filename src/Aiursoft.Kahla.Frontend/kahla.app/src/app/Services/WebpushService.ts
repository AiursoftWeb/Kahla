import { Injectable } from '@angular/core';
import { DevicesApiService } from './Api/DevicesApiService';
import { PushSubscriptionSetting } from '../Models/PushSubscriptionSetting';
import { CacheService } from './CacheService';
import { urlBase64ToUint8Array } from '../Utils/StringUtils';
import { lastValueFrom } from 'rxjs';
import { mapDeviceName } from '../Utils/UaMapper';
import { SwalToast } from '../Utils/Toast';
import { ServiceWorkerIpcMessage } from '../Models/ServiceWorker/ServiceWorkerIpc';
import { Router } from '@angular/router';
import { Logger } from './Logger';

@Injectable({
    providedIn: 'root',
})
export class WebpushService {
    constructor(
        private devicesApiService: DevicesApiService,
        private cacheService: CacheService,
        private router: Router,
        private logger: Logger
    ) {}

    public get serviceWorkerAvailable(): boolean {
        return 'serviceWorker' in navigator && !('__TAURI__' in window);
    }

    public get requireUserApproval(): boolean {
        return 'Notification' in window && Notification.permission === 'default';
    }

    public get notificationAvail(): boolean {
        return (
            this.serviceWorkerAvailable &&
            'Notification' in window &&
            Notification.permission === 'granted'
        );
    }

    public get pushSettings(): PushSubscriptionSetting {
        const storage = JSON.parse(
            localStorage.getItem('setting-pushSubscription') ?? 'null'
        ) as PushSubscriptionSetting | null;
        if (!storage) {
            return {
                enabled: true,
                deviceId: 0,
            };
        }
        return storage;
    }

    private updatePushSettings(value: PushSubscriptionSetting) {
        localStorage.setItem('setting-pushSubscription', JSON.stringify(value));
    }

    public async bindDevice(pushSubscription: PushSubscription, subscriptionChanged = false) {
        const settings = this.pushSettings;
        if (!settings.enabled) return;
        if (settings.deviceId) {
            if (!subscriptionChanged) return;
            this.logger.info('Update push subscription for device...');
            await lastValueFrom(
                this.devicesApiService.UpdateDevice(
                    settings.deviceId,
                    navigator.userAgent,
                    pushSubscription.endpoint,
                    pushSubscription.toJSON().keys!.p256dh,
                    pushSubscription.toJSON().keys!.auth
                )
            );
            this.logger.ok('Device updated.');
        } else {
            this.logger.info('Bind device for push subscription...');
            const resp = await lastValueFrom(
                this.devicesApiService.AddDevice(
                    navigator.userAgent,
                    pushSubscription.endpoint,
                    pushSubscription.toJSON().keys!.p256dh,
                    pushSubscription.toJSON().keys!.auth
                )
            );
            this.updatePushSettings({
                enabled: true,
                deviceId: resp.value,
            });
            this.logger.ok('Device binded.');
        }
    }

    public async unbindDevice() {
        if (this.pushSettings.enabled) return;
        if (this.pushSettings.deviceId) {
            this.logger.info('Unbind device for push subscription...');
            await lastValueFrom(this.devicesApiService.DropDevice(this.pushSettings.deviceId));
            this.updatePushSettings({
                enabled: false,
                deviceId: 0,
            });
            this.logger.ok('Device unbinded.');
        }
    }

    public async getDeviceList() {
        const resp = await lastValueFrom(this.devicesApiService.MyDevices());
        resp.items.forEach(t => {
            [t._os, t._browser] = mapDeviceName(t.name) ?? ['Unknown OS', 'Unknown Browser'];
        });
        // should check if current device id has already been invalid
        if (
            this.pushSettings.deviceId &&
            !resp.items.find(t => t.id === this.pushSettings.deviceId)
        ) {
            // invalid id, remove it
            this.updatePushSettings({
                ...this.pushSettings,
                deviceId: 0,
            });
            this.logger.info('Device id invalid, rotating.');
            void this.subscribeUser(); // Re subscribe to rebind the device; Fire and forget
        }
        return resp.items;
    }

    public async webpushInit() {
        if (!this.notificationAvail || !this.pushSettings.enabled) return;
        await this.subscribeUser();
        this.initSubscriptionUpdater();

        await this.getDeviceList(); // Get the device list to make sure current device haven't been removed
    }

    public async updateEnabled(enabled: boolean) {
        this.updatePushSettings({
            ...this.pushSettings,
            enabled: enabled,
        });
        if (enabled) {
            await this.subscribeUser(true);
        } else {
            await this.unbindDevice();
        }
    }

    //#region Push subscription

    private get pushServerMetadata() {
        if (!this.cacheService.serverConfig) {
            throw new Error('Server config not loaded');
        }
        return {
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(
                this.cacheService.serverConfig.vapidPublicKey
            ),
        };
    }

    public async subscribeUser(forceUpdate = false) {
        if (!this.notificationAvail) return;
        const registration = await navigator.serviceWorker.ready;
        let sub = await registration.pushManager.getSubscription();
        if (sub && forceUpdate) {
            this.logger.info('Unsubscribing existing push subscription...');
            await sub.unsubscribe();
            sub = null;
            this.logger.ok('Push subscription unsubscribed.');
        }
        if (!sub) {
            this.logger.info('Push subscription not exists, creating new one.');
            sub = await registration.pushManager.subscribe(this.pushServerMetadata);
            this.logger.ok('Push subscription created.');
        }
        await this.bindDevice(sub);
    }

    public async unsubscribeUser() {
        if (!this.notificationAvail) return;
        const registration = await navigator.serviceWorker.ready;
        const sub = await registration.pushManager.getSubscription();
        if (sub) {
            await sub.unsubscribe();
        }
    }

    public initSubscriptionUpdater() {
        if (!this.notificationAvail) return;
        navigator.serviceWorker.addEventListener('pushsubscriptionchange', async () => {
            this.logger.info('Push subscription changed');
            const registration = await navigator.serviceWorker.ready;
            const sub = await registration.pushManager.subscribe(this.pushServerMetadata);
            await this.bindDevice(sub, true);
            this.logger.ok('Push subscription updated');
        });
    }

    public async requestUserApproval() {
        if (!this.requireUserApproval) return;
        const permission = await Notification.requestPermission();
        if (permission === 'granted') {
            this.logger.ok('Notification permission granted.');
        } else {
            this.logger.error('Notification permission denied.');
        }
    }

    //#endregion

    //#region Service Worker Registration
    public async registerServiceWorker() {
        this.logger.info('Registering service worker...');
        if (!this.serviceWorkerAvailable) {
            this.logger.info('ServiceWorker not available in this environment, skipping.');
            return;
        }
        try {
            const registration = await navigator.serviceWorker.register('/sw.js');
            this.logger.ok(
                'ServiceWorker registration successful with scope: ',
                registration.scope
            );
            // https://stackoverflow.com/questions/37573482/to-check-if-serviceworker-is-in-waiting-state
            // The page has been loaded when there's already a waiting and active SW.
            // This would happen if skipWaiting() isn't being called, and there are
            // still old tabs open.
            if (registration.waiting && registration.active) {
                this.alertUpdate();
            } else {
                // updatefound is also fired for the very first install. ¯\_(ツ)_/¯
                registration.addEventListener('updatefound', () => {
                    registration.installing?.addEventListener('statechange', () => {
                        if (registration.active && registration.waiting) {
                            // If there's already an active SW, and skipWaiting() is not
                            // called in the SW, then the user needs to close all their
                            // tabs before they'll get updates.
                            this.alertUpdate();
                        } else {
                            // Otherwise, this newly installed SW will soon become the
                            // active SW. Rather than explicitly wait for that to happen,
                            // just show the initial "content is cached" message.
                            this.logger.ok('Content is cached for the first time!');
                        }
                    });
                });
            }

            navigator.serviceWorker.addEventListener('message', ev => {
                const data = ev.data as ServiceWorkerIpcMessage;
                if (data.type === 'navigate') {
                    void this.router.navigateByUrl(data.url);
                }
            });
        } catch (err) {
            this.logger.error('ServiceWorker registration failed: ', err);
            return;
        }

        await this.requestUserApproval();
    }

    private alertUpdate() {
        this.logger.info('ServiceWorker update detected.');
        setTimeout(() => {
            void SwalToast.fire({
                icon: 'info',
                position: 'bottom-right',
                title: 'A new version of the Kahla is ready.',
                text: 'Please close and reopen all the page of the app to update.\nJust reloading the page cannot update the app.',
            });
        }, 1000); // Delay fire to ensure the page is fully loaded and not to disturb the user
    }
    //#endregion
}
