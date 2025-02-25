import { Component, OnInit } from '@angular/core';
import Swal from 'sweetalert2';
import { Device } from '../Models/Device';
import { DevicesApiService } from '../Services/Api/DevicesApiService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { WebpushService } from '../Services/WebpushService';
import { PushSubscriptionSetting } from '../Models/PushSubscriptionSetting';
import { SwalToast, WorkingDialog } from '../Utils/Toast';

@Component({
    templateUrl: '../Views/devices.html',
    styleUrls: ['../Styles/menu.scss'],
    standalone: false,
})
export class DevicesComponent implements OnInit {
    constructor(
        public webpushService: WebpushService,
        public devicesApiService: DevicesApiService
    ) {}

    public currentSettings: PushSubscriptionSetting;
    public devices: Device[];

    public ngOnInit() {
        this.currentSettings = this.webpushService.pushSettings;
        void this.updateDeviceList();
    }

    public async updateDeviceList() {
        this.devices = await this.webpushService.getDeviceList();
    }

    public detail(device: Device): void {
        void Swal.fire({
            title: 'Device detail',
            html:
                '<table style="margin: auto;"><tr><th>Add IP</th><td>' +
                device.ipAddress +
                '</td></tr><tr><th>Add time</th><td>' +
                new Date(device.addTime).toLocaleString() +
                '</td></tr></table>',
        });
    }

    public async testPush() {
        try {
            const result = await lastValueFrom(this.devicesApiService.PushTestMessage());
            void Swal.fire('Successfully sent!', result.message, 'info');
            setTimeout(() => {
                void this.updateDeviceList();
            }, 1000);
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    public async setWebPushStatus(value: boolean) {
        if (value) {
            void WorkingDialog.fire({
                title: 'Subscribing you to webpush notification...',
                text: "Please don't close the page.",
            });
        }
        try {
            await this.webpushService.updateEnabled(value);
            if (value) {
                Swal.close();
            }
            void SwalToast.fire('Updated!');
            this.currentSettings = this.webpushService.pushSettings;
            void this.updateDeviceList();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    public getElectronNotify(): boolean {
        return localStorage.getItem('setting-electronNotify') !== 'false';
    }

    public setElectronNotify(value: boolean) {
        localStorage.setItem('setting-electronNotify', value ? 'true' : 'false');
    }
}
