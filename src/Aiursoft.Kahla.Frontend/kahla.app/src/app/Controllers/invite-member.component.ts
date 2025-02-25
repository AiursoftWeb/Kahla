import { Component, effect, input, signal } from '@angular/core';
import {
    MyContactsRepository,
    MyContactsRepositoryFiltered,
} from '../Repositories/MyContactsRepository';
import { ContactsApiService } from '../Services/Api/ContactsApiService';
import { RepositoryListBase } from '../Repositories/RepositoryBase';
import { ContactInfo } from '../Models/Contacts/ContactInfo';

@Component({
    templateUrl: '../Views/invite-member.html',
    styleUrls: ['../Styles/search-part.scss'],
    standalone: false,
})
export class InviteMemberComponent {
    readonly id = input.required<number>();

    readonly searchText = signal<string>('');
    readonly idsToInvite = signal<string[]>([]);
    contactsRepo: RepositoryListBase<ContactInfo>;

    constructor(myContactsRepo: MyContactsRepository, contactsApiService: ContactsApiService) {
        effect(() => {
            if (this.searchText().length > 0) {
                this.contactsRepo = new MyContactsRepositoryFiltered(
                    contactsApiService,
                    this.searchText()
                );
                void this.contactsRepo.updateAll();
            } else {
                this.contactsRepo = myContactsRepo;
            }
        });
    }
}
