import { lastValueFrom } from 'rxjs';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import { ContactsApiService } from '../Services/Api/ContactsApiService';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';
import { Injectable } from '@angular/core';

export class MyContactsRepositoryFiltered extends RepositoryListBase<ContactInfo> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private contactsApiService: ContactsApiService,
        private keyword?: string,
        private keywordExclude?: string
    ) {
        super();
    }

    protected async requestOnline(take: number, skip: number): Promise<[ContactInfo[], number]> {
        const resp = await lastValueFrom(
            this.contactsApiService.List(take, skip, this.keyword, this.keywordExclude)
        );
        return [resp.knownContacts, resp.totalKnownContacts];
    }
}

@Injectable({
    providedIn: 'root',
})
export class MyContactsRepository extends MyContactsRepositoryFiltered {
    protected readonly persistConfig: RepositoryPersistConfig = {
        name: 'contacts',
        version: 1,
        persist: true,
    };

    constructor(contactsApiService: ContactsApiService) {
        super(contactsApiService);
    }
}
