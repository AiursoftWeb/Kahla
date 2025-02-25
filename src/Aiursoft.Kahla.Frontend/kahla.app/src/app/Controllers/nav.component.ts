import { Component } from '@angular/core';
import { CacheService } from '../Services/CacheService';
import { HomeService } from '../Services/HomeService';

@Component({
    selector: 'app-nav',
    templateUrl: '../Views/nav.html',
    styleUrls: ['../Styles/nav.scss', '../Styles/reddot.scss'],
    standalone: false,
})
export class NavComponent {
    constructor(
        public cacheService: CacheService,
        public homeService: HomeService
    ) {}
}
