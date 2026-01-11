import {
    afterRenderEffect,
    Component,
    effect,
    HostListener,
    input,
    resource,
    signal,
} from '@angular/core';
import { MessageService } from '../Services/MessageService';
import { CacheService } from '../Services/CacheService';
import { MessageContent } from '../Models/Messages/MessageContent';
import { ParsedMessage } from '../Models/Messages/ParsedMessage';
import { lastValueFrom } from 'rxjs';
import { MessagesApiService } from '../Services/Api/MessagesApiService';
import { scrollBottom } from '../Utils/Scrolling';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { KahlaMessagesRepo } from '@aiursoft/kahla.sdk';
import { ThreadInfoCacheDictionary } from '../Caching/ThreadInfoCacheDictionary';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { MyThreadsOrderedRepository } from '../Repositories/MyThreadsOrderedRepository';
import { EventService } from '../Services/EventService';
import { isThreadPropertyChangedEvent, ThreadPropertyChangedEvent } from '../Models/Events/ThreadPropertyChangedEvent';
import { filter } from 'rxjs';

@Component({
    templateUrl: '../Views/talking.html',
    styleUrls: ['../Styles/talking.scss'],
    standalone: false,
})
export class TalkingComponent {
    public repo?: KahlaMessagesRepo;
    public parsedMessages = signal<ParsedMessage[]>([]);
    public showPanel = signal(false);
    public hasNewMessages = signal(false);

    private firstPull?: number;

    // route input
    public id = input.required<number>();

    public threadInfo = resource({
        request: () => this.id(),
        loader: ({ request }) => {
            try {
                return this.threadInfoCacheDictionary.get(request);
            } catch (err) {
                showCommonErrorDialog(err);
                throw err;
            }
        },
    });

    constructor(
        public messageService: MessageService,
        public cacheService: CacheService,
        messageApiService: MessagesApiService,
        public threadApiService: ThreadsApiService,
        public threadInfoCacheDictionary: ThreadInfoCacheDictionary,
        myThreadsOrderedRepository: MyThreadsOrderedRepository,
        private eventService: EventService
    ) {
        effect(async cleanup => {
            if (!this.id()) return;
            this.parsedMessages.set([]);
            // Obtain the websocket connection token
            try {
                const resp = await lastValueFrom(messageApiService.InitThreadWebsocket(this.id()));
                this.repo = new KahlaMessagesRepo(resp.webSocketEndpoint, true);
                const sub = this.repo.messages.messages.onChange.subscribe(event => {
                    const newItem = ParsedMessage.fromCommit(event.newNode.value);
                    switch (event.type) {
                        case 'addFirst':
                            this.parsedMessages.set([newItem, ...this.parsedMessages()]);
                            break;
                        case 'addLast':
                            this.parsedMessages.set([...this.parsedMessages(), newItem]);
                            break;
                        case 'addBefore':
                            {
                                const lastIndex = this.parsedMessages().findLastIndex(
                                    t => t.id === event.refNode!.value.id
                                );
                                if (lastIndex !== -1) {
                                    this.parsedMessages.set([
                                        ...this.parsedMessages().slice(0, lastIndex),
                                        newItem,
                                        ...this.parsedMessages().slice(lastIndex),
                                    ]);
                                }
                            }
                            break;
                    }
                });
                this.repo.connect();
                this.firstPull = Date.now();
                cleanup(() => {
                    sub.unsubscribe();
                    this.repo?.disconnect();
                });
            } catch (err) {
                showCommonErrorDialog(err);
            }
        });

        effect(cleanup => {
            const threadId = this.id();
            const sub = this.eventService.onMessage
                .pipe(filter(t => isThreadPropertyChangedEvent(t)))
                .subscribe(t => {
                    const ev = t as ThreadPropertyChangedEvent;
                    if (ev.threadId === threadId) {
                        void this.threadInfo.reload();
                    }
                });
            cleanup(() => sub.unsubscribe());
        });

        effect(() => {
            if (!this.threadInfo.value()) return;
            myThreadsOrderedRepository.clearUnreadFor(this.threadInfo.value()!.id);
        });

        afterRenderEffect(() => {
            this.parsedMessages();
            if (this.firstPull && Date.now() - this.firstPull < 5000) {
                this.firstPull = undefined;
                scrollBottom(true);
            }
            if (!scrollBottom(true, 500)) {
                // User ignored new messages
                this.hasNewMessages.set(true);
            }
        });
    }

    @HostListener('window:scroll', [])
    onScroll() {
        const belowWindowPercent =
            (document.documentElement.scrollHeight -
                window.scrollY -
                document.documentElement.clientHeight) /
            document.documentElement.clientHeight;
        if (belowWindowPercent <= 0) {
            this.hasNewMessages.set(false);
        }
    }

    public loadMore() {
        const oldScrollHeight = document.documentElement.scrollHeight;
        setTimeout(() => {
            window.scroll(0, document.documentElement.scrollHeight - oldScrollHeight);
        }, 0);
    }

    public send({ content, ats }: { content: MessageContent; ats?: string[] }) {
        if (!this.repo || !this.cacheService.mine()) return;
        this.repo?.send({
            content: JSON.stringify(content),
            senderId: this.cacheService.mine()!.me.id,
            preview: this.messageService.buildPreview(content),
            ats: ats,
        });
    }
}
