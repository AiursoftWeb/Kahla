import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/';
import { ApiService } from './ApiService';
import { ThreadsListApiResponse } from '../../Models/Threads/ThreadsListApiResponse';
import { AiurValueNamed } from '../../Models/AiurValue';
import { ThreadOptions } from '../../Models/Threads/ThreadOptions';
import { ThreadInfo, ThreadInfoJoined } from '../../Models/Threads/ThreadInfo';
import { AiurProtocol } from '../../Models/AiurProtocal';
import { ThreadMembersApiResponse } from '../../Models/Threads/ThreadMembersApiResponse';

@Injectable()
export class ThreadsApiService {
    private static serverPath = '/threads';

    constructor(private apiService: ApiService) {}

    public Search(
        take = 20,
        skip = 0,
        searchInput?: string,
        excluding?: string
    ): Observable<ThreadsListApiResponse> {
        return this.apiService.Get(ThreadsApiService.serverPath + '/search', {
            take,
            skip,
            searchInput,
            excluding,
        });
    }

    public DetailsJoined(id: number): Observable<AiurValueNamed<ThreadInfoJoined, 'thread'>> {
        return this.apiService.Get(ThreadsApiService.serverPath + `/details-joined/${id}`, {});
    }

    public DetailsAnnoymous(id: number): Observable<AiurValueNamed<ThreadInfo, 'thread'>> {
        return this.apiService.Get(ThreadsApiService.serverPath + `/details-annoymous/${id}`, {});
    }

    public HardInvite(userId: string): Observable<AiurValueNamed<number, 'newThreadId'>> {
        return this.apiService.Post(ThreadsApiService.serverPath + `/hard-invite/${userId}`, {});
    }

    public CreateScratch(
        options: Omit<ThreadOptions, 'iconFilePath'>
    ): Observable<AiurValueNamed<number, 'newThreadId'>> {
        return this.apiService.Post(ThreadsApiService.serverPath + '/create-scratch', options);
    }

    public SetMute(id: number, value: boolean): Observable<AiurProtocol> {
        return this.apiService.Post(ThreadsApiService.serverPath + `/set-mute/${id}`, {
            mute: value,
        });
    }

    public Members(id: number, take: number, skip: number): Observable<ThreadMembersApiResponse> {
        return this.apiService.Get(ThreadsApiService.serverPath + `/members/${id}`, { take, skip });
    }

    public Leave(id: number): Observable<AiurProtocol> {
        return this.apiService.Post(ThreadsApiService.serverPath + `/leave/${id}`, {});
    }

    public Dissolve(id: number): Observable<AiurProtocol> {
        return this.apiService.Post(ThreadsApiService.serverPath + `/dissolve/${id}`, {});
    }

    public Transfer(id: number, targetUserId: string): Observable<AiurProtocol> {
        return this.apiService.Post(ThreadsApiService.serverPath + `/transfer-ownership/${id}`, {
            targetUserId,
        });
    }

    public UpdateThread(id: number, options: ThreadOptions): Observable<AiurProtocol> {
        return this.apiService.Patch(ThreadsApiService.serverPath + `/update-thread/${id}`, {...options});
    }
}
