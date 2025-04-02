import { Component, computed, effect, input, linkedSignal, resource } from '@angular/core';
import { Router } from '@angular/router';
import { SwalToast, YesNoDialog } from '../Utils/Toast';
import { ContactsApiService } from '../Services/Api/ContactsApiService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { MyContactsRepository } from '../Repositories/MyContactsRepository';
import { BlocksApiService } from '../Services/Api/BlocksApiService';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { CommonThreadRepository } from '../Repositories/CommonThreadsRepository';
import Swal from 'sweetalert2';

@Component({
    templateUrl: '../Views/user.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/button.scss'],
    standalone: false,
})
export class UserComponent {
    readonly id = input.required<string>();
    readonly info = resource({
        request: () => this.id(),
        loader: async ({ request: id }) => {
            try {
                return await lastValueFrom(this.contactsApiService.Details(id, 0, 0));
            } catch (err) {
                showCommonErrorDialog(err);
                return null;
            }
        },
    });
    readonly isCommonThreadsShown = linkedSignal<boolean>(() => !!this.info.value() && false);
    readonly commonThreadsRepo = computed(() =>
        this.info.value()
            ? new CommonThreadRepository(
                  this.contactsApiService,
                  this.info.value()!.searchedUser.user.id
              )
            : null
    );

    constructor(
        private contactsApiService: ContactsApiService,
        private threadsApiService: ThreadsApiService,
        private blocksApiService: BlocksApiService,
        private router: Router,
        private myContactsRepository: MyContactsRepository
    ) {
        effect(() => {
            if (
                this.isCommonThreadsShown() &&
                this.commonThreadsRepo()?.status === 'uninitialized'
            ) {
                void this.commonThreadsRepo()!.updateAll();
            }
        });
    }

    public async delete(id: string) {
        const resp = await YesNoDialog.fire({
            title: 'Are you sure to delete the contact?',
        });
        if (!resp.value) return;
        try {
            await lastValueFrom(this.contactsApiService.Remove(id));
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }
        void SwalToast.fire('Success', '', 'success');
        void this.myContactsRepository.updateAll();
        void this.router.navigate(['/home']);
    }

    public async addAsContact() {
        try {
            await lastValueFrom(
                this.contactsApiService.Add(this.info.value()!.searchedUser.user.id)
            );
            void SwalToast.fire('Success', '', 'success');
            this.info.reload();
            void this.myContactsRepository.updateAll();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    public async report() {
        const resp = await Swal.fire({
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
        });
        if (!resp.value) return;
        try {
            await lastValueFrom(
                this.contactsApiService.Report(
                    this.info.value()!.searchedUser.user.id,
                    resp.value as string
                )
            );
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }
    }

    public shareUser() {
        // this.router.navigate(['/share-target', {message: `[user]${this.info.id}`}]);
    }

    public message() {
        void this.router.navigate([`/talking/${this.info.value()!.defaultThread!}`]);
    }

    public messageId(id: number) {
        void this.router.navigate([`/talking/${id}`]);
    }

    public async newThread() {
        // hard invite
        const resp = await lastValueFrom(
            this.threadsApiService.HardInvite(this.info.value()!.searchedUser.user.id)
        );
        void SwalToast.fire('Thread Created.', '', 'success');
        void this.router.navigate([`/talking/${resp.newThreadId}`]);
    }

    public async block() {
        const resp = await YesNoDialog.fire({
            title: `Are you sure to ${this.info.value()!.searchedUser.isBlockedByYou ? 'unblock' : 'block'} this user?`,
            text: `Blocked user will not be able to create new threads with you.`,
        });
        if (!resp.value) return;
        try {
            await lastValueFrom(
                this.info.value()!.searchedUser.isBlockedByYou
                    ? this.blocksApiService.Remove(this.info.value()!.searchedUser.user.id)
                    : this.blocksApiService.Block(this.info.value()!.searchedUser.user.id)
            );
        } catch (err) {
            showCommonErrorDialog(err);
            return;
        }
        void SwalToast.fire();
        this.info.reload();
        void this.myContactsRepository.updateAll();
    }
}
