import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/';
import { AiurProtocol } from '../../Models/AiurProtocol';
import { UserDetailViewModel } from '../../Models/ApiModels/UserDetailViewModel';
import { ApiService } from './ApiService';
import { ContactsListApiResponse } from '../../Models/Contacts/ContactsListApiResponse';
import { UserInfoViewModel } from '../../Models/ApiModels/UserInfoViewModel';

@Injectable()
export class ContactsApiService {
    private static serverPath = '/contacts';

    constructor(private apiService: ApiService) {}

    public List(
        take = 20,
        skip = 0,
        searchInput?: string,
        excluding?: string
    ): Observable<ContactsListApiResponse> {
        return this.apiService.Get(ContactsApiService.serverPath + '/list', {
            take,
            skip,
            searchInput,
            excluding,
        });
    }

    public Add(id: string): Observable<AiurProtocol> {
        return this.apiService.Post(ContactsApiService.serverPath + `/add/${id}`, {});
    }

    public Remove(id: string): Observable<AiurProtocol> {
        return this.apiService.Post(ContactsApiService.serverPath + `/remove/${id}`, {});
    }

    public Details(id: string, take = 1, skip = 0): Observable<UserDetailViewModel> {
        return this.apiService.Get(ContactsApiService.serverPath + `/details/${id}`, {
            take,
            skip,
        });
    }

    public Info(id: string): Observable<UserInfoViewModel> {
        return this.apiService.Get(ContactsApiService.serverPath + `/info/${id}`, {});
    }

    public Report(targetUserId: string, reason: string): Observable<AiurProtocol> {
        return this.apiService.Post(ContactsApiService.serverPath + `/report/`, {
            TargetUserId: targetUserId,
            Reason: reason,
        });
    }
}
