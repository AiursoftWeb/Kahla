import { lastValueFrom } from 'rxjs';
import { KahlaUser } from '../Models/KahlaUser';
import { ContactsApiService } from '../Services/Api/ContactsApiService';
import { CachedDictionaryBase } from './CachedDictionaryBase';
import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class UserInfoCacheDictionary extends CachedDictionaryBase<string, KahlaUser> {
    constructor(private contactsApiService: ContactsApiService) {
        super(3600, 'user-brief-info');
    }

    protected async cacheMiss(key: string): Promise<KahlaUser> {
        const resp = await lastValueFrom(this.contactsApiService.Info(key));
        return resp.briefUser;
    }
}
