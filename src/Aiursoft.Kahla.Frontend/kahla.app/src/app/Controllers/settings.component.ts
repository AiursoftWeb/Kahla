import { Component } from '@angular/core';
import { AuthApiService } from '../Services/Api/AuthApiService';
import { Router } from '@angular/router';
import { InitService } from '../Services/InitService';
import Swal from 'sweetalert2';
import { CacheService } from '../Services/CacheService';
import { HomeService } from '../Services/HomeService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { WebpushService } from '../Services/WebpushService';
import { YesNoDialog } from '../Utils/Toast';

@Component({
    selector: 'app-settings',
    templateUrl: '../Views/settings.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/button.scss'],
    standalone: false,
})
export class SettingsComponent {
    constructor(
        private authApiService: AuthApiService,
        private webpushService: WebpushService,
        private router: Router,
        private initService: InitService,
        public cacheService: CacheService,
        public homeService: HomeService
    ) {}

    public pwaAddHomeScreen(): void {
        /* eslint-disable @typescript-eslint/no-unsafe-member-access */
        /* eslint-disable @typescript-eslint/no-unsafe-call */
        this.homeService.pwaHomeScreenPrompt.prompt();
        this.homeService.pwaHomeScreenPrompt.userChoice.then(choiceResult => {
            if (choiceResult.outcome === 'accepted') {
                this.homeService.pwaHomeScreenSuccess = true;
            }
            this.homeService.pwaHomeScreenPrompt = null;
        });
        /* eslint-enable @typescript-eslint/no-unsafe-member-access */
        /* eslint-enable @typescript-eslint/no-unsafe-call */
    }

    public async SignOut() {
        const willSignOut = await YesNoDialog.fire({
            title: 'Are you sure to sign out?',
        });

        if (!willSignOut.value) return;

        try {
            await lastValueFrom(
                this.authApiService.Signout(this.webpushService.pushSettings.deviceId)
            );
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }

        this.initService.destroy();
        void this.router.navigate(['/signin'], { replaceUrl: true });
        void this.webpushService.unsubscribeUser();
    }

    public async sendEmail() {
        if (!this.cacheService.mine() && this.cacheService.mine()!.me.emailConfirmed) return;
        const sendEmail = await Swal.fire({
            title: 'Please verify your email.',
            text: "Please confirm your email as soon as possible! Or you may lose access \
                            to your account in a few days! Without confirming your email, you won't receive \
                            any important notifications and cannot reset your password!",
            icon: 'warning',
            confirmButtonText: 'Send Email',
            showCancelButton: true,
        });
        if (sendEmail.isDismissed) return;
        try {
            await lastValueFrom(this.authApiService.SendMail(this.cacheService.mine()!.me.email));
            void Swal.fire({
                title: 'Please check your inbox.',
                text: 'Email was send to ' + this.cacheService.mine()!.me.email,
                icon: 'success',
            });
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }
}
