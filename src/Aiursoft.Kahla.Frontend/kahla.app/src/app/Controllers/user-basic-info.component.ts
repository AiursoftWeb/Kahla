import { Component, input } from '@angular/core';
import { KahlaUser } from '../Models/KahlaUser';

@Component({
    selector: 'app-user-basic-info',
    templateUrl: '../Views/user-basic-info.html',
    standalone: false,
    styleUrls: ['../Styles/menu.scss', '../Styles/reddot.scss', '../Styles/badge.scss'],
})
export class UserBasicInfoComponent {
    user = input.required<KahlaUser>();
    online = input<boolean>();
}
