import { Injectable } from '@angular/core';
import { ApiService } from './ApiService';
import { BlocksListApiResponse } from '../../Models/Blocks/BlocksListApiResponse';
import { Observable } from 'rxjs';
import { AiurProtocol } from '../../Models/AiurProtocol';

@Injectable()
export class BlocksApiService {
    private static serverPath = '/blocks';

    constructor(private apiService: ApiService) {}

    public List(
        take = 20,
        skip = 0,
        searchInput?: string,
        excluding?: string
    ): Observable<BlocksListApiResponse> {
        return this.apiService.Get(BlocksApiService.serverPath + '/list', {
            take,
            skip,
            searchInput,
            excluding,
        });
    }

    public Block(id: string): Observable<AiurProtocol> {
        return this.apiService.Post(BlocksApiService.serverPath + `/block/${id}`, {});
    }

    public Remove(id: string): Observable<AiurProtocol> {
        return this.apiService.Post(BlocksApiService.serverPath + `/remove/${id}`, {});
    }
}
