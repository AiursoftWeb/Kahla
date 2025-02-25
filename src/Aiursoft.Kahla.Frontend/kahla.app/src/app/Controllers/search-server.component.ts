import { Component, effect, signal } from '@angular/core';
import { SearchApiService } from '../Services/Api/SearchApiService';
import { ServerContactsRepository } from '../Repositories/ServerContactsRepository';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { ServerThreadsRepository } from '../Repositories/ServerThreadsRepository';
import { ThreadInfo } from '../Models/Threads/ThreadInfo';
import { Router } from '@angular/router';

@Component({
    templateUrl: '../Views/search-server.html',
    styleUrls: ['../Styles/search-part.scss', '../Styles/button.scss', '../Styles/reddot.scss'],
    standalone: false,
})
export class SearchServerComponent {
    searchTerm = signal('');
    selectedTab = signal(0);

    public contactsRepo?: ServerContactsRepository = undefined;
    public threadsRepo?: ServerThreadsRepository = undefined;

    constructor(
        private searchApiService: SearchApiService,
        private router: Router
    ) {
        effect(() => {
            if (this.searchTerm().length > 0) {
                void this.search(this.searchTerm());
            }
        });
    }

    public async search(term: string) {
        this.contactsRepo = new ServerContactsRepository(this.searchApiService, term);
        this.threadsRepo = new ServerThreadsRepository(this.searchApiService, term);
        try {
            await Promise.all([this.contactsRepo.updateAll(), this.threadsRepo.updateAll()]);
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    threadClicked({ thread }: { thread: ThreadInfo }) {
        void this.router.navigate(['/thread-public', thread.id]);
    }
}
