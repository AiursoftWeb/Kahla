import { auditTime, filter, lastValueFrom, Subject, Subscription } from 'rxjs';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { RepositoryBase } from './RepositoryBase';
import { Injectable } from '@angular/core';
import { EventService } from '../Services/EventService';
import { isThreadAddedEvent } from '../Models/Events/ThreadAddedEvent';
import { isThreadRemovedEvent } from '../Models/Events/ThreadRemovedEvent';
import { isThreadPropertyChangedEvent, ThreadPropertyChangedEvent } from '../Models/Events/ThreadPropertyChangedEvent';
import { KahlaEventType } from '../Models/Events/EventType';
import { NewMessageEvent } from '../Models/Events/NewMessageEvent';
import { ThreadInfoCacheDictionary } from '../Caching/ThreadInfoCacheDictionary';
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root',
})
export class MyThreadsOrderedRepository extends RepositoryBase<ThreadInfoJoined> {
    protected readonly persistConfig = {
        name: 'mythreads-ordered',
        version: 1,
        persist: true,
    };
    private _canLoadMore = true;
    private sub?: Subscription;
    private saveCacheTrigger$ = new Subject<void>();

    constructor(
        private threadsApiService: ThreadsApiService,
        private eventService: EventService,
        private threadInfoCacheDictionary: ThreadInfoCacheDictionary,
        private router: Router
    ) {
        super();
        this.saveCacheTrigger$.pipe(auditTime(2000)).subscribe(() => {
            this.saveCache();
        });
    }

    private updateCache(threads: ThreadInfoJoined[]) {
        threads.forEach(t => this.threadInfoCacheDictionary.set(t.id, t));
    }

    protected async updateAllInternal(): Promise<void> {
        // Unsubscribe from previous events
        this.sub?.unsubscribe();

        // Load the first 20 threads
        this.data = (await lastValueFrom(this.threadsApiService.Mine(20))).knownThreads;
        this.updateCache(this.data);
        this._canLoadMore = this.data.length >= 20;

        // Start subscribing to message events
        this.sub = this.eventService.onMessage
            .pipe(filter(t => isThreadAddedEvent(t)))
            .subscribe(t => {
                this.data = [t.thread, ...this.data];
                this.threadInfoCacheDictionary.set(t.thread.id, t.thread);
                this.saveCacheTrigger$.next();
            });

        this.sub.add(
            this.eventService.onMessage.pipe(filter(t => isThreadRemovedEvent(t))).subscribe(t => {
                this.data = this.data.filter(d => d.id !== t.threadId);
                this.saveCacheTrigger$.next();
            })
        );

        this.sub.add(
            this.eventService.onMessage.pipe(filter(t => isThreadPropertyChangedEvent(t))).subscribe(t => {
                const ev = t as ThreadPropertyChangedEvent;
                const thread = this.data.find(d => d.id === ev.threadId);
                if (thread) {
                    thread.name = ev.threadName;
                    thread.imagePath = ev.threadImagePath;
                    this.threadInfoCacheDictionary.set(thread.id, thread);
                    this.saveCacheTrigger$.next();
                } else {
                    // Even if not in current data, we still want to update the cache
                    // But we don't want to trigger a fetch if it's not there.
                    // However, set() will just overwrite or add.
                    // Since we don't have the full ThreadInfo object here (ev only has name and image),
                    // we can't easily update it if we don't already have it.
                    // But if it's in the cache, we might want to update it.
                }
            })
        );

        this.sub.add(
            this.eventService.onMessage
                .pipe(filter(t => t.type === KahlaEventType.NewMessage))
                .subscribe(async t => {
                    const ev = t as NewMessageEvent;
                    const threadId = ev.message.threadId;
                    let thread = this.data.find(t => t.id === threadId);
                    if (thread) {
                        // We will move the thread to the top
                        this.data = this.data.filter(t => t.id !== threadId);
                    } else {
                        thread = await this.threadInfoCacheDictionary.get(threadId);
                    }
                    thread.messageContext.latestMessage = ev.message;
                    if (
                        !this.router.isActive(`/talking/${threadId}`, {
                            paths: 'exact',
                            queryParams: 'ignored',
                            fragment: 'ignored',
                            matrixParams: 'ignored',
                        })
                    ) {
                        ++thread.messageContext.unReadAmount;
                        if (ev.mentioned) {
                            thread.unreadAtMe = true;
                        }
                    }
                    this.data = [thread, ...this.data];
                    this.threadInfoCacheDictionary.set(threadId, thread);
                    this.saveCacheTrigger$.next();
                })
        );

        this.sub.add(
            this.eventService.onErrorOrClose.subscribe(() => {
                this.status = 'offline';
            })
        );

        this.sub.add(
            this.eventService.onReconnect.subscribe(() => {
                void this.updateAll();
            })
        );
    }

    public get canLoadMore() {
        return this._canLoadMore;
    }

    protected async loadMoreInternal(take: number): Promise<void> {
        const lastId = this.data.at(-1)?.id;
        const newData = await lastValueFrom(this.threadsApiService.Mine(take, lastId));
        this.updateCache(newData.knownThreads);
        // the data might be changed since then
        // find the lastId, and append the new data after it. Discard any items after the lastId
        this.data = [
            ...this.data.slice(0, this.data.findLastIndex(t => t.id === lastId) + 1),
            ...newData.knownThreads,
        ];
    }

    public clearUnreadFor(threadId: number) {
        const thread = this.data.find(t => t.id === threadId);
        if (thread) {
            thread.messageContext.unReadAmount = 0;
            thread.unreadAtMe = false;
            this.threadInfoCacheDictionary.set(threadId, thread);
            this.saveCacheTrigger$.next();
        }
    }
}
