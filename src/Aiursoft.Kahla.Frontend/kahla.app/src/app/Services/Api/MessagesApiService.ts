import { Injectable } from '@angular/core';
import { ApiService } from './ApiService';
import { InitWebsocketViewModel } from '../../Models/ApiModels/InitPusherViewModel';
import { Observable } from 'rxjs';

@Injectable()
export class MessagesApiService {
    private static serverPath = '/messages';

    constructor(private apiService: ApiService) {}

    public InitWebsocket(): Observable<InitWebsocketViewModel> {
        return this.apiService.Post(MessagesApiService.serverPath + '/init-websocket', {});
    }

    public InitThreadWebsocket(threadId: number): Observable<InitWebsocketViewModel> {
        return this.apiService.Post(
            MessagesApiService.serverPath + `/init-thread-websocket/${threadId}`,
            {}
        );
    }
}
