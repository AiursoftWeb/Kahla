import { Component, input, model, output } from '@angular/core';

@Component({
    selector: 'app-editable-menu-item',
    templateUrl: '../Views/editable-menu-item.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/menu-textbox.scss'],
    standalone: false,
})
export class EditableMenuItemComponent {
    readonly title = input.required<string>();
    readonly iconClasses = input.required<string>();
    readonly placeholder = input<string>('');

    value = model.required<string>();
    confirm = output<string>();

    editing = false;
    editValue = '';

    startEdit() {
        this.editValue = this.value();
        this.editing = true;
    }

    cancelEdit() {
        this.editing = false;
    }

    saveEdit() {
        this.value.set(this.editValue);
        this.confirm.emit(this.editValue);
        this.editing = false;
    }
}
