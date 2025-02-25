import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { KahlaMessagesMemoryStore } from './KahlaMessagesMemoryStore.js';
import type { ChatMessage } from './Models/ChatMessage.js';
import type { KahlaCommit } from './Models/KahlaCommit.js';
import { Subject } from 'rxjs';

function deserializeCommit<T>(content: string): KahlaCommit<T>[] {
    const parsed = JSON.parse(content) as KahlaCommit<T>[];
    // Json doesn't support Date type, so we need to convert it back
    parsed.forEach(t => (t.commitTime = new Date(t.commitTime as unknown as string)));
    return parsed;
}

export class KahlaMessagesRepo {
    private _webSocket: WebSocketSubject<KahlaCommit<ChatMessage>[]> | null = null;
    messages: KahlaMessagesMemoryStore = new KahlaMessagesMemoryStore();

    private reconnectDelay = 1000;
    private shouldOpen = false;

    public onConnected = new Subject<Event>();
    public onIncomingPacketProcessed = new Subject<void>();
    public onError = new Subject<void>();

    constructor(
        private webSocketEndpoint: string,
        private autoReconnect = false
    ) {
        this.onConnected.subscribe(() => {
            this.reconnectDelay = 1000; // Reset reconnect delay after a successful connection
            setTimeout(() => {
                if (this.health) {
                    this.push();
                }
            }, 1000); // Delay 1 second to push messages
        });
    }

    connect(): KahlaMessagesRepo {
        // start: The first offset of commit that I want.
        this.shouldOpen = true;
        const realEndpoint = `${this.webSocketEndpoint}?start=${this.messages.pulledItemsOffset}`;
        this._webSocket = webSocket({
            url: realEndpoint,
            deserializer: t => deserializeCommit(t.data as string),
            openObserver: this.onConnected,
        });
        this._webSocket.subscribe({
            next: message => void this.onNewWebSocketMessage(message),
            error: err => {
                // rollback push pointer. We could not guarantee that the server has received the message we pushed just now.
                this.messages.lastPushed = this.messages.lastPulled;
                this.messages.pushedItemsOffset = this.messages.pulledItemsOffset;
                console.error(err);
                this.onError.next();
            },
            complete: () => {
                console.log('WebSocket connection closed');
                this._webSocket = null;

                if (this.autoReconnect && this.shouldOpen) {
                    setTimeout(() => this.connect(), this.reconnectDelay);
                    this.reconnectDelay = Math.max(2 * this.reconnectDelay, 30000); // Exponential backoff
                }
            },
        });
        return this;
    }

    async send(message: ChatMessage): Promise<void> {
        await this.messages.commit(message);
        this.push();
    }

    async commitOnly(message: ChatMessage): Promise<void> {
        await this.messages.commit(message);
    }

    getAllMessages(): Iterable<KahlaCommit<ChatMessage>> {
        return this.messages.getAllMessagesEnumerable();
    }

    head(): KahlaCommit<ChatMessage> | null {
        return this.messages.getHead();
    }

    disconnect(): void {
        this.shouldOpen = false;
        if (this._webSocket) {
            this._webSocket.complete();
        }
        this._webSocket = null;
    }

    private async onNewWebSocketMessage(content: KahlaCommit<ChatMessage>[]): Promise<void> {
        // const commits = deserializeCommit<ChatMessage>(content);
        for (const commit of content) {
            await this.messages.onPulledMessage(commit);
        }
        this.onIncomingPacketProcessed.next();
    }

    push(): void {
        if (!this._webSocket) {
            throw new Error('No websocket connection');
        }
        const messages = [...this.messages.push()];
        if (messages.length > 0) {
            // const content = serialize(messages);
            this._webSocket.next(messages);
        }
    }

    public get health(): boolean {
        return !!this._webSocket && !this._webSocket.closed;
    }
}
