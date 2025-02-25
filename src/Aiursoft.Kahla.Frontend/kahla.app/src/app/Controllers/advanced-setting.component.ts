import { Component, OnInit } from '@angular/core';
import { AuthApiService } from '../Services/Api/AuthApiService';
import { KahlaUser } from '../Models/KahlaUser';
import Swal from 'sweetalert2';
import { CacheService } from '../Services/CacheService';
import { Subscription } from 'rxjs';
import { AppOptions } from '../Models/AppOptions';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';

@Component({
    templateUrl: '../Views/advanced-settings.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/button.scss'],
    standalone: false,
})
export class AdvancedSettingComponent implements OnInit {
    public me: KahlaUser;
    public options: AppOptions;
    public updatingSetting?: Subscription;

    constructor(
        private authApiService: AuthApiService,
        private cacheService: CacheService
    ) {}

    async ngOnInit() {
        const res = await this.cacheService?.mineCache.get();
        this.me = Object.assign({}, res.me);
        this.options = Object.assign({}, res.privateSettings);
    }

    public updateSettings(): void {
        if (this.updatingSetting && !this.updatingSetting.closed) {
            this.updatingSetting.unsubscribe();
            this.updatingSetting = undefined;
        }
        this.updatingSetting = this.authApiService
            .UpdateMe({
                enableEmailNotification: this.options.enableEmailNotification,
                enableEnterToSendMessage: this.options.enableEnterToSendMessage,
                enableHideMyOnlineStatus: this.options.enableHideMyOnlineStatus,
                allowSearchByName: this.options.allowSearchByName,
                allowHardInvitation: this.options.allowHardInvitation,
            })
            .subscribe(
                () => {
                    this.updatingSetting = undefined;
                    this.cacheService.mineCache.set({ me: this.me, privateSettings: this.options });
                },
                err => {
                    this.updatingSetting = undefined;
                    showCommonErrorDialog(err);
                }
            );
    }

    public todo(): void {
        void Swal.fire(
            'Under development',
            'This features is still under development ^_^.',
            'info'
        );
    }
}
