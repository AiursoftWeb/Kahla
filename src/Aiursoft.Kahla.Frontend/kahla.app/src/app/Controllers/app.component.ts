import { Component, HostListener } from '@angular/core';
import { InitService } from '../Services/InitService';
import Swal from 'sweetalert2';
import { ThemeService } from '../Services/ThemeService';
import { Router } from '@angular/router';
import { HomeService } from '../Services/HomeService';
import { CacheService } from '../Services/CacheService';
import { WebpushService } from '../Services/WebpushService';

@Component({
    selector: 'app-kahla',
    templateUrl: '../Views/app.html',
    styleUrls: ['../Styles/app.scss'],
    standalone: false,
})
export class AppComponent {
    constructor(
        private initService: InitService,
        private webpushService: WebpushService,
        private themeService: ThemeService,
        public cacheService: CacheService,
        public route: Router,
        public homeService: HomeService
    ) {
        // Temporary apply the local theme setting
        this.themeService.ApplyThemeFromLocal();
        void this.initService.init();
    }

    @HostListener('window:popstate', [])
    onPopstate() {
        Swal.close();
    }

    @HostListener('window:load', [])
    onLoad() {
        void this.webpushService.registerServiceWorker();
    }

    @HostListener('window:beforeinstallprompt', ['$event'])
    onbeforeinstallprompt(e: unknown) {
        this.homeService.pwaHomeScreenPrompt = e;
    }
}
