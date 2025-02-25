import {
    Component,
    EventEmitter,
    input,
    Input,
    model,
    output,
    Output,
    TemplateRef,
} from '@angular/core';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import { RepositoryLike } from '../Repositories/RepositoryBase';

export interface ContactListItem extends ContactInfo {
    tags?: {
        desc: string;
        type: 'owner' | 'admin'; // Add new types here, then add the corresponding CSS class in badge.scss
    }[];
}

@Component({
    selector: 'app-contact-list',
    templateUrl: '../Views/contact-list.html',
    styleUrls: [
        '../Styles/contact-list.scss',
        '../Styles/reddot.scss',
        '../Styles/button.scss',
        '../Styles/badge.scss',
    ],
    standalone: false,
})
export class ContactListComponent {
    @Input() public contacts?: RepositoryLike<ContactListItem>;

    @Input() public emptyMessage = 'No results.';

    public contextMenu = input<TemplateRef<unknown> | null>(null);
    public selectable = input<'single' | 'multi' | null>(null);
    public selectedIds = model<string[]>([]);
    public preventDefault = input<boolean>(false);

    @Output() public contactClicked = new EventEmitter<{
        item: ContactInfo;
        secondary: boolean;
    }>();

    public contextMenuClicked = output<ContactListItem>();

    public userClick(contact: ContactInfo, secondary: boolean) {
        this.contactClicked.emit({
            item: contact,
            secondary,
        });
        if (this.selectable()) {
            if (this.selectedIds().includes(contact.user.id)) {
                this.selectedIds.set(this.selectedIds().filter(id => id !== contact.user.id));
            } else {
                if (this.selectable() === 'single') {
                    this.selectedIds.set([contact.user.id]);
                } else {
                    this.selectedIds.set([...this.selectedIds(), contact.user.id]);
                }
            }
        }
    }
}
