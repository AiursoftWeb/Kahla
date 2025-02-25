import { Component, effect, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CacheService } from '../Services/CacheService';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import {
    MyContactsRepository,
    MyContactsRepositoryFiltered,
} from '../Repositories/MyContactsRepository';
import { RepositoryListBase } from '../Repositories/RepositoryBase';
import { ContactsApiService } from '../Services/Api/ContactsApiService';

@Component({
    selector: 'app-friends',
    templateUrl: '../Views/friends.html',
    styleUrls: [
        '../Styles/menu.scss',
        '../Styles/reddot.scss',
        '../Styles/search-part.scss',
        '../Styles/friends.scss',
        '../Styles/button.scss',
        '../Styles/badge.scss',
    ],
    standalone: false,
})
export class FriendsComponent implements OnInit {
    public searchTxt = signal('');
    public searchType = signal(0);

    private detailLoading = false;
    public contactsRepo?: RepositoryListBase<ContactInfo>;

    constructor(
        private router: Router,
        public cacheService: CacheService,
        public myContactsRepository: MyContactsRepository,
        contactsApiService: ContactsApiService
    ) {
        effect(() => {
            if (this.searchTxt().length > 0) {
                this.contactsRepo = new MyContactsRepositoryFiltered(
                    contactsApiService,
                    this.searchTxt()
                );
                void this.contactsRepo.updateAll();
            } else {
                this.contactsRepo = this.myContactsRepository;
            }
        });
    }

    public ngOnInit(): void {
        if (this.cacheService.mine() && this.myContactsRepository.health) {
            void this.myContactsRepository.updateAll();
        }
    }

    public goSingleSearch(): void {
        // TODO
        // if (this.showUsers) {
        //     if (this.results.users.length === 1) {
        //         this.userClick(this.results.users[0], ctrl);
        //     }
        // } else {
        //     if (this.results.threads.length === 1) {
        //         this.groupClick(this.results.threads[0], ctrl);
        //     }
        // }
    }

    public userClick(user: ContactInfo, ctrl: boolean) {
        if (ctrl) {
            void this.router.navigate(['/user', user.user.id]);
        } else {
            if (this.detailLoading) {
                return;
            }
            this.detailLoading = true;
            // this.friendsApiService.UserDetail(user.id).subscribe(p => {
            //     this.router.navigate(['/talking', p.conversationId]);
            //     this.detailLoading = false;
            // });
        }
    }
}
