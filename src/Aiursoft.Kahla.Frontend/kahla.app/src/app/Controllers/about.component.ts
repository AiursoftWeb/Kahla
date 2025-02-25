import { Component } from '@angular/core';
import { Values } from '../values';
import { ApiService } from '../Services/Api/ApiService';
import { environment } from '../../environments/environment';
import { CacheService } from '../Services/CacheService';

@Component({
    templateUrl: '../Views/about.html',
    styleUrls: ['../Styles/about.scss', '../Styles/menu.scss', '../Styles/button.scss'],
    standalone: false,
})
export class AboutComponent {
    public sourceCodeURL = Values.sourceCodeURL;
    public website = environment.serversProvider;

    constructor(
        public apiService: ApiService,
        public cacheService: CacheService
    ) {}

    public getCurrentYear(): number {
        return new Date().getFullYear();
    }
}
