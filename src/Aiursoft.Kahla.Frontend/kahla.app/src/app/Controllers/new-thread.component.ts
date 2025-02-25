import { Component, signal } from '@angular/core';
import { ThreadOptions } from '../Models/Threads/ThreadOptions';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { Router } from '@angular/router';
import { SwalToast } from '../Utils/Toast';

@Component({
    templateUrl: '../Views/new-thread.html',
    styleUrls: ['../Styles/button.scss'],
    standalone: false,
})
export class NewThreadComponent {
    options = signal<ThreadOptions>({
        name: 'Scratch Thread',
        allowDirectJoinWithoutInvitation: false,
        allowMembersEnlistAllMembers: true,
        allowMemberSoftInvitation: true,
        allowMembersSendMessages: true,
        allowSearchByName: false,
    });

    constructor(
        private threadsApiService: ThreadsApiService,
        private router: Router
    ) {}

    async create() {
        try {
            const resp = await lastValueFrom(this.threadsApiService.CreateScratch(this.options()));
            void SwalToast.fire({ title: 'Thread created.', icon: 'success' });
            void this.router.navigate([`/talking/${resp.newThreadId}`]);
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }
}
