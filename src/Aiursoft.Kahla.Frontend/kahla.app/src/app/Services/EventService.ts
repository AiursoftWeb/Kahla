import { Injectable } from '@angular/core';
import { lastValueFrom, Subject } from 'rxjs';
import { KahlaEvent } from '../Models/Events/KahlaEvent';
import { MessagesApiService } from './Api/MessagesApiService';

@Injectable({
    providedIn: 'root',
})
export class EventService {
    private ws: WebSocket;
    public connecting = false;
    public errorOrClose: boolean;

    public onMessage: Subject<KahlaEvent> = new Subject<KahlaEvent>();
    public onErrorOrClose: Subject<boolean> = new Subject<boolean>();
    public onReconnect: Subject<void> = new Subject<void>();

    private timeoutNumber = 1000;
    private reconnectAttemptTimeout: ReturnType<typeof setTimeout>;
    private closeWebSocket = false;

    constructor(private messagesApiService: MessagesApiService) {}

    public async initPusher(): Promise<void> {
        this.connecting = true;
        this.closeWebSocket = false;
        try {
            const model = await lastValueFrom(this.messagesApiService.InitWebsocket());
            if (this.ws) {
                this.closeWebSocket = true;
                this.ws.close();
            }
            this.closeWebSocket = false;
            this.ws = new WebSocket(model.webSocketEndpoint);
            this.ws.onopen = () => {
                this.connecting = false;
                clearTimeout(this.reconnectAttemptTimeout);
                if (this.errorOrClose) {
                    this.errorOrClose = false;
                    this.timeoutNumber = 1000;
                    this.onReconnect.next();
                }
            };
            this.ws.onmessage = evt => {
                this.onMessage.next(JSON.parse(evt.data as string) as KahlaEvent);
            };
            this.ws.onerror = () => {
                this.errorOrClosedFunc();
                this.onErrorOrClose.next(true);
            };
            this.ws.onclose = () => {
                this.errorOrClosedFunc();
                this.onErrorOrClose.next(false);
            };
        } catch {
            this.errorOrClosedFunc();
        }
    }

    private errorOrClosedFunc(): void {
        if (!this.closeWebSocket) {
            this.connecting = false;
            this.errorOrClose = true;
            this.fireNetworkAlert();
            this.autoReconnect();
        }
    }

    private autoReconnect(): void {
        this.reconnectAttemptTimeout = setTimeout(() => {
            if (this.timeoutNumber < 10000) {
                this.timeoutNumber += 1000;
            }
            this.attemptReconnect();
        }, this.timeoutNumber);
    }

    public attemptReconnect() {
        if (this.errorOrClose) {
            void this.initPusher();
        }
    }

    public fireNetworkAlert(): void {
        console.warn('Websocket connection down.');
    }

    public destroyConnection() {
        this.closeWebSocket = true;
        this.ws.close();
        clearTimeout(this.reconnectAttemptTimeout);
    }
}
