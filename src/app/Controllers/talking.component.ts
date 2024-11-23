﻿import {
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
import { uuid4 } from '../Utils/Uuid';
import { MessageContent } from '../Models/Messages/MessageContent';
import { ParsedMessage } from '../Models/Messages/ParsedMessage';
import { lastValueFrom } from 'rxjs';
import { MessagesApiService } from '../Services/Api/MessagesApiService';
import { scrollBottom } from '../Utils/Scrolling';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { KahlaMessagesRepo } from '@aiursoft/kahla-sdk.js';

@Component({
    templateUrl: '../Views/talking.html',
    styleUrls: ['../Styles/talking.scss'],
    standalone: false,
})
export class TalkingComponent {
    private windowInnerHeight = 0;
    private formerWindowInnerHeight = 0;
    private keyBoardHeight = 0;
    private chatInputHeight: number;
    public lastAutoLoadMoreTimestamp = 0;

    public repo?: KahlaMessagesRepo;
    public parsedMessages = signal<ParsedMessage[]>([]);
    public showPanel = signal(false);

    public hasNewMessages = signal(false);
    // route input
    public threadId = input.required<number>({
        alias: 'id',
    });

    public threadInfo = resource({
        request: () => this.threadId(),
        loader: async ({ request }) => {
            if (request)
                return (await lastValueFrom(this.threadApiService.DetailsJoined(request))).thread;
        },
    });

    constructor(
        public messageService: MessageService,
        public cacheService: CacheService,
        private messageApiService: MessagesApiService,
        public threadApiService: ThreadsApiService
    ) {
        effect(async cleanup => {
            if (!this.threadId()) return;
            this.parsedMessages.set([]);
            // Obtain the websocket connection token
            const resp = await lastValueFrom(
                messageApiService.InitThreadWebsocket(this.threadId())
            );
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
            cleanup(() => {
                sub.unsubscribe();
                this.repo.disconnect();
            });
        });

        afterRenderEffect(() => {
            this.parsedMessages();
            if (!scrollBottom(true, 500)) {
                // User ignored new messages
                this.hasNewMessages.set(true);
            }
        });
    }

    @HostListener('window:resize', [])
    onResize() {
        if (window.innerHeight < this.windowInnerHeight) {
            this.keyBoardHeight = this.windowInnerHeight - window.innerHeight;
            window.scroll(0, window.scrollY + this.keyBoardHeight);
        } else if (
            window.innerHeight - this.formerWindowInnerHeight > 100 &&
            this.messageService.belowWindowPercent > 0.2
        ) {
            window.scroll(0, window.scrollY - this.keyBoardHeight);
        } else if (window.innerHeight - this.formerWindowInnerHeight > 100) {
            window.scroll(0, window.scrollY);
        }
        this.formerWindowInnerHeight = window.innerHeight;
    }

    @HostListener('window:scroll', [])
    onScroll() {
        this.messageService.updateBelowWindowPercent();
        if (this.messageService.belowWindowPercent <= 0) {
            this.hasNewMessages.set(false);
        }
        // if (
        //     window.scrollY <= 0 &&
        //     document.documentElement.scrollHeight > document.documentElement.clientHeight + 100 &&
        //     this.messageService.conversation &&
        //     !this.messageService.messageLoading
        // ) {
        //     const now = Date.now();
        //     const interval =
        //         this.messageService.showMessagesCount < this.messageService.localMessages.length
        //             ? 10
        //             : 2000;
        //     if (this.lastAutoLoadMoreTimestamp + interval < now) {
        //         this.loadMore();
        //         this.lastAutoLoadMoreTimestamp = now;
        //     } else {
        //         setTimeout(
        //             () => this.onScroll(),
        //             this.lastAutoLoadMoreTimestamp + interval + 10 - now
        //         );
        //     }
        // }
    }


    public getAtListMaxHeight(): number {
        return window.innerHeight - this.chatInputHeight - 106;
    }

    public async loadMore() {
        const oldScrollHeight = document.documentElement.scrollHeight;
        setTimeout(() => {
            window.scroll(0, document.documentElement.scrollHeight - oldScrollHeight);
        }, 0);
    }

    public send({ content }: { content: MessageContent }) {
        // this.temp_demo_msg.push(
        //     new ParsedMessage(uuid4(), content, this.cacheService.cachedData.me.id, new Date())
        // );
        this.repo.send({
            content: JSON.stringify(content),
            senderId: this.cacheService.cachedData.me.id ?? uuid4(),
            preview: this.messageService.buildPreview(content),
        });
    }
}
