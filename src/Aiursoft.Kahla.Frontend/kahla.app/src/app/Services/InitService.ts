import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { CacheService } from './CacheService';
import { ThemeService } from './ThemeService';
import Swal from 'sweetalert2';
import { ApiService } from './Api/ApiService';
import { EventService } from './EventService';
import { GlobalNotifyService } from './GlobalNotifyService';
import { lastValueFrom } from 'rxjs';
import { MyContactsRepository } from '../Repositories/MyContactsRepository';
import { WebpushService } from './WebpushService';
import { MyThreadsOrderedRepository } from '../Repositories/MyThreadsOrderedRepository';
import { HttpErrorResponse } from '@angular/common/http';
import { Logger } from './Logger';

@Injectable({
    providedIn: 'root',
})
export class InitService {
    constructor(
        private apiService: ApiService,
        private router: Router,
        private cacheService: CacheService,
        private themeService: ThemeService,
        private webpushService: WebpushService,
        private eventService: EventService,
        private globalNotifyService: GlobalNotifyService,
        private myContactsRepository: MyContactsRepository,
        private myThreadsOrderedRepository: MyThreadsOrderedRepository,
        private logger: Logger
    ) {}

    public async init(): Promise<void> {
        this.logger.info('Welcome to Kahla.App!');

        this.myContactsRepository.initCache();
        this.myThreadsOrderedRepository.initCache();
        this.logger.ok('Local cache initialized.');

        // load server config
        this.cacheService.serverConfig = await lastValueFrom(this.apiService.ServerInfo());

        if (this.cacheService.serverConfig) {
            try {
                await this.cacheService.mineCache.update();
            } catch (error) {
                if (error instanceof HttpErrorResponse && error.status === 401) {
                    // Unauthorized, user not sign in
                    this.logger.warn('User not signed in. Redirecting to signin page.', error);
                    void this.router.navigate(['/signin'], { replaceUrl: true });
                    return;
                } else {
                    this.logger.warn('Network not avail. Cannot update my info.', error);
                }
            }

            this.logger.ok('User signed in.');
            if (this.router.isActive('/signin', false)) {
                void this.router.navigate(['/home'], { replaceUrl: true });
            }

            // Webpush Service
            void this.webpushService.webpushInit();

            // Init global push
            void this.eventService.initPusher();
            this.globalNotifyService.init();

            // Load User Info
            // this.cacheService.cachedData.me.avatarURL =
            //     this.probeService.encodeProbeFileUrl(this.cacheService.cachedData.me.iconFilePath);
            this.themeService.ApplyThemeFromRemote(this.cacheService.mine()!.privateSettings);

            // Fire and forget updates
            void this.myContactsRepository.updateAll();
            void this.myThreadsOrderedRepository.updateAll();
        } else {
            void this.router.navigate(['/signin'], { replaceUrl: true });
            void Swal.fire('Server is not available', 'Please try again later.', 'error');
        }
    }

    public destroy(): void {
        this.eventService.destroyConnection();
        localStorage.clear();
    }
}
