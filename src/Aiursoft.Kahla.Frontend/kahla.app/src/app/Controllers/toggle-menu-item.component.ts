import { Component, model, input } from '@angular/core';

@Component({
    selector: 'app-toggle-menu-item',
    templateUrl: '../Views/toggle-menu-item.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/toggleButton.scss'],
    standalone: false,
})
export class ToggleMenuItemComponent {
    readonly title = input('option');
    readonly iconClasses = input('fas fa-cogs');
    value = model.required<boolean>();
}
