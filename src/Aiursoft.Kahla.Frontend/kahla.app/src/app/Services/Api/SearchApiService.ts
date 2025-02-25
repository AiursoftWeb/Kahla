import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserSearchResult } from '../../Models/Search/UserSearchResult';
import { ApiService } from './ApiService';
import { ThreadSearchResult } from '../../Models/Search/ThreadSearchResult';

@Injectable()
export class SearchApiService {
    private static serverPath = '/search';

    constructor(private apiService: ApiService) {}

    public SearchUsers(
        searchInput: string,
        take = 20,
        skip = 0,
        excluding?: string
    ): Observable<UserSearchResult> {
        return this.apiService.Get(SearchApiService.serverPath + `/search-users`, {
            searchInput,
            take,
            skip,
            excluding,
        });
    }

    public SearchThreads(
        searchInput: string,
        take = 20,
        skip = 0,
        excluding?: string
    ): Observable<ThreadSearchResult> {
        return this.apiService.Get(SearchApiService.serverPath + `/search-threads`, {
            searchInput,
            take,
            skip,
            excluding,
        });
    }
}
