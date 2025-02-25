import { Component, EventEmitter, Input, Output, input } from '@angular/core';
import { RepositoryBase } from '../Repositories/RepositoryBase';
import { ThreadInfo, ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { CacheService } from '../Services/CacheService';

@Component({
    selector: 'app-threads-list',
    templateUrl: '../Views/threads-list.html',
    styleUrls: ['../Styles/threads-list.scss', '../Styles/reddot.scss'],
    standalone: false,
})
export class ThreadsListComponent {
    @Input() public threadRepo: RepositoryBase<ThreadInfo>;

    public readonly emptyMessage = input('No results.');
    public readonly highlightPredicate = input<(item: ThreadInfo) => boolean>(() => false);
    public readonly externalView = input(false);
    public readonly scrollingContainer = input<HTMLElement | null>(null);
    public readonly autoLoadMore = input(false);

    @Output() public threadClicked = new EventEmitter<{
        thread: ThreadInfo;
        secondary: boolean;
    }>();

    constructor(public cacheService: CacheService) {}

    public onlineStatusOf(thread: ThreadInfoJoined): boolean | null {
        if (!this.cacheService?.mine()?.me || thread.topTenMembers.length > 2) return null;
        if (thread.topTenMembers.length === 1) return true;
        return thread.topTenMembers
            .filter(t => t.user.id !== this.cacheService.mine()?.me?.id)
            .some(t => t.online);
    }

    public asJoined(thread: ThreadInfo): ThreadInfoJoined {
        return thread as ThreadInfoJoined;
    }
}
