import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CacheService } from '../Services/CacheService';
import Swal from 'sweetalert2';
import { Values } from '../values';
import { SwalToast } from '../Utils/Toast';
import { ApiService } from '../Services/Api/ApiService';
import { ContactsApiService } from '../Services/Api/ContactsApiService';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { MyContactsRepository } from '../Repositories/MyContactsRepository';
import { BlocksApiService } from '../Services/Api/BlocksApiService';

@Component({
    templateUrl: '../Views/user.html',
    styleUrls: [
        '../Styles/menu.scss',
        '../Styles/button.scss',
        '../Styles/reddot.scss',
        '../Styles/badge.scss',
    ],
})
export class UserComponent implements OnInit {
    public info: ContactInfo;
    public loadingImgURL = Values.loadingImgURL;

    constructor(
        private route: ActivatedRoute,
        private contactsApiService: ContactsApiService,
        private blocksApiService: BlocksApiService,
        private router: Router,
        private myContactsRepository: MyContactsRepository,
        public cacheService: CacheService,
        public apiService: ApiService
    ) {}

    public ngOnInit(): void {
        this.route.params.subscribe(t => {
            this.updateFriendInfo(t.id);
        });
    }

    public updateFriendInfo(userId: string) {
        this.contactsApiService.Details(userId).subscribe(response => {
            this.info = response.searchedUser;
        });
        // this.friendsApiService.UserDetail(userId).subscribe(response => {
        //     // this.info = response.user;
        //     // this.areFriends = response.areFriends;
        //     // this.conversationId = response.conversationId;
        //     // this.sentRequest = response.sentRequest;
        //     // this.pendingRequest = response.pendingRequest;
        //     this.info.avatarURL = this.probeService.encodeProbeFileUrl(this.info.iconFilePath);
        // });
    }

    public async delete(id: string) {
        const resp = await Swal.fire({
            title: 'Are you sure to delete a friend?',
            icon: 'warning',
            showCancelButton: true,
        });
        if (!resp.value) return;
        try {
            await lastValueFrom(this.contactsApiService.Remove(id));
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }
        SwalToast.fire('Success', '', 'success');
        this.myContactsRepository.updateAll();
        this.router.navigate(['/home']);
    }

    public async addAsContract() {
        try {
            await lastValueFrom(this.contactsApiService.Add(this.info.user.id));
            SwalToast.fire('Success', '', 'success');
            this.updateFriendInfo(this.info.user.id);
            this.myContactsRepository.updateAll();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    public report(): void {
        Swal.fire({
            title: 'Report',
            input: 'textarea',
            inputPlaceholder: 'Type your reason here...',
            inputAttributes: {
                maxlength: '200',
            },
            confirmButtonColor: 'red',
            showCancelButton: true,
            confirmButtonText: 'Report',
            inputValidator: inputValue => {
                if (!inputValue || inputValue.length < 5) {
                    return "The reason's length should between five and two hundreds.";
                }
            },
        }).then(result => {
            if (!result.dismiss) {
                // this.friendsApiService.Report(this.info.id, result.value as string).subscribe(response => {
                //     if (response.code === 0) {
                //         Swal.fire('Success', response.message, 'success');
                //     } else {
                //         Swal.fire('Error', response.message, 'error');
                //     }
                // }, () => {
                //     Swal.fire('Error', 'Report error.', 'error');
                // });
            }
        });
    }

    public shareUser() {
        // this.router.navigate(['/share-target', {message: `[user]${this.info.id}`}]);
    }

    public message() {
        if (this.info.commonThreads.length === 0) {
            this.router.navigate(['/talking', this.info.commonThreads[0].id]);
        }
    }

    public async block() {
        const resp = await Swal.fire({
            title: `Are you sure to ${this.info.isBlockedByYou ? 'unblock' : 'block'} this user?`,
            icon: 'warning',
            showCancelButton: true,
        });
        if (!resp.value) return;
        try {
            await lastValueFrom(
                this.info.isBlockedByYou
                    ? this.blocksApiService.Remove(this.info.user.id)
                    : this.blocksApiService.Block(this.info.user.id)
            );
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }
        SwalToast.fire('Success', '', 'success');
        this.updateFriendInfo(this.info.user.id);
        this.myContactsRepository.updateAll();
    }
}
