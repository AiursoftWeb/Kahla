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
import { MessageStorageService } from '../Services/MessageStorageService';

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

    public pulledIndex = signal(0);
    public pushedIndex = signal(0);

    /** Whether older messages can be loaded from storage */
    public hasMoreMessages = signal(false);
    /** Whether messages are currently being loaded */
    public loadingMessages = signal(false);
    /** Current minimum sequence index loaded (for pagination) */
    private minLoadedSeqIndex = 0;

    private firstPull?: number;
    private storageCleanup?: () => void;

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
        private messageApiService: MessagesApiService,
        public threadApiService: ThreadsApiService,
        public threadInfoCacheDictionary: ThreadInfoCacheDictionary,
        private myThreadsOrderedRepository: MyThreadsOrderedRepository,
        private eventService: EventService,
        private messageStorageService: MessageStorageService
    ) {
        effect(async cleanup => {
            if (!this.id()) return;
            this.parsedMessages.set([]);
            this.hasMoreMessages.set(false);
            this.minLoadedSeqIndex = 0;

            const threadId = this.id();

            try {
                // Step 1: Load cached messages from IndexedDB first
                const cachedData = await this.messageStorageService.loadInitialMessages(threadId);
                
                if (cachedData.messages.length > 0) {
                    // Display cached messages immediately
                    const cachedParsed = cachedData.messages.map(m => ParsedMessage.fromCommit(m));
                    this.parsedMessages.set(cachedParsed);
                    this.hasMoreMessages.set(cachedData.hasMore);
                    
                    // Track the minimum sequence index for pagination
                    // Use the pulledItemsOffset minus the number of cached messages
                    if (cachedData.syncState) {
                        this.minLoadedSeqIndex = Math.max(
                            0,
                            cachedData.syncState.pulledItemsOffset - cachedData.messages.length
                        );
                    }
                }

                // Step 2: Obtain the websocket connection token
                const resp = await lastValueFrom(this.messageApiService.InitThreadWebsocket(threadId));
                
                // Step 3: Create repo with restored sync state
                this.repo = new KahlaMessagesRepo(resp.webSocketEndpoint, true);
                
                // Restore sync state offsets if available
                if (cachedData.syncState) {
                    this.repo.messages.pulledItemsOffset = cachedData.syncState.pulledItemsOffset;
                    this.repo.messages.pushedItemsOffset = cachedData.syncState.pushedItemsOffset;
                }

                // Step 4: Subscribe to message changes for UI updates
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

                // Step 5: Subscribe to pointer changes for UI sync indicator
                const sub2 = this.repo.messages.onPointersChanged.subscribe(() => {
                    this.pulledIndex.set(this.repo!.messages.pulledItemsOffset);
                    this.pushedIndex.set(this.repo!.messages.pushedItemsOffset);
                });

                // Step 6: Subscribe to repo for automatic persistence to IndexedDB
                this.storageCleanup = this.messageStorageService.subscribeToRepo(threadId, this.repo);

                // Step 7: Connect to WebSocket
                this.repo.connect();
                this.firstPull = Date.now();

                cleanup(() => {
                    sub.unsubscribe();
                    sub2.unsubscribe();
                    this.storageCleanup?.();
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
            this.myThreadsOrderedRepository.clearUnreadFor(this.threadInfo.value()!.id);
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

    /**
     * Load older messages from IndexedDB storage.
     * Called when user scrolls up or clicks "Load more".
     */
    public async loadMoreMessages(): Promise<void> {
        if (this.loadingMessages() || !this.hasMoreMessages()) return;

        this.loadingMessages.set(true);
        const oldScrollHeight = document.documentElement.scrollHeight;

        try {
            const result = await this.messageStorageService.loadOlderMessages(
                this.id(),
                this.minLoadedSeqIndex
            );

            if (result.messages.length > 0) {
                // Prepend older messages to the list
                const olderParsed = result.messages.map(m => ParsedMessage.fromCommit(m));
                this.parsedMessages.set([...olderParsed, ...this.parsedMessages()]);
                
                // Update pagination state
                this.minLoadedSeqIndex = result.loadedFrom;
                this.hasMoreMessages.set(result.hasMore);

                // Maintain scroll position after prepending
                setTimeout(() => {
                    window.scroll(0, document.documentElement.scrollHeight - oldScrollHeight);
                }, 0);
            } else {
                this.hasMoreMessages.set(false);
            }
        } catch (err) {
            console.error('Failed to load older messages:', err);
        } finally {
            this.loadingMessages.set(false);
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
