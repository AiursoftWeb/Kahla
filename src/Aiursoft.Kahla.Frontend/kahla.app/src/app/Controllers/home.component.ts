import { Component } from '@angular/core';
import { HomeService } from '../Services/HomeService';

@Component({
    selector: 'app-home',
    templateUrl: '../Views/home.html',
    standalone: false,
})
export class HomeComponent {
    constructor(public homeService: HomeService) {}
}
