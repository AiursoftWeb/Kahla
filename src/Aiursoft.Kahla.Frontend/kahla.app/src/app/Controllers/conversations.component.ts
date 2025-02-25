import { Component, effect, signal } from '@angular/core';
import { ThreadInfo } from '../Models/Threads/ThreadInfo';
import { Router } from '@angular/router';
import { CacheService } from '../Services/CacheService';
import { MyThreadsRepositoryFiltered } from '../Repositories/ThreadsRepository';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { MyThreadsOrderedRepository } from '../Repositories/MyThreadsOrderedRepository';
import { RepositoryBase } from '../Repositories/RepositoryBase';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';

@Component({
    selector: 'app-conversations',
    templateUrl: '../Views/conversations.html',
    styleUrls: ['../Styles/search-part.scss'],
    standalone: false,
})
export class ConversationsComponent {
    public threadsRepo?: RepositoryBase<ThreadInfo>;
    searchText = signal('');

    constructor(
        private router: Router,
        public cacheService: CacheService,
        public myThreadsOrderedRepository: MyThreadsOrderedRepository,
        private threadsApiService: ThreadsApiService
    ) {
        effect(() => {
            if (this.searchText()) {
                this.threadsRepo = new MyThreadsRepositoryFiltered(
                    this.threadsApiService,
                    this.searchText()
                );
                this.threadsRepo.updateAll().catch(showCommonErrorDialog);
            } else {
                this.threadsRepo = this.myThreadsOrderedRepository;
            }
        });
    }

    public detail(info: ThreadInfo): void {
        void this.router.navigate(['/detail', info.id]);
    }

    public current(info: ThreadInfo): boolean {
        return new RegExp(`^.+/${info.id}(/.*)*$`, 'g').test(this.router.url);
    }

    public goTalking(id: number) {
        void this.router.navigate(['/talking', id]);
    }
}
