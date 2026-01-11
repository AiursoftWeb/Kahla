import { Component, effect, input, linkedSignal, resource } from '@angular/core';
import { ThreadInfoCacheDictionary } from '../Caching/ThreadInfoCacheDictionary';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { ThreadOptions } from '../Models/Threads/ThreadOptions';
import { pickProperties } from '../Utils/ObjectUtils';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { filter, lastValueFrom, Subscription } from 'rxjs';
import { SwalToast } from '../Utils/Toast';
import { EventService } from '../Services/EventService';
import { isThreadPropertyChangedEvent, ThreadPropertyChangedEvent } from '../Models/Events/ThreadPropertyChangedEvent';

@Component({
    selector: 'app-manage-thread',
    templateUrl: '../Views/manage-thread.html',
    styleUrls: ['../Styles/manage-thread.scss'],
    standalone: false,
})
export class ManageThreadComponent {
    id = input.required<number>();
    public updatingSetting?: Subscription;

    threadInfo = resource({
        request: () => this.id(),
        loader: ({ request }) =>
            this.threadInfoCacheDictionary
                .get(request)
                .catch(err => void showCommonErrorDialog(err)),
    });

    threadProfile = linkedSignal<ThreadOptions>(() => {
        if (!this.threadInfo.value()) return {} as ThreadOptions;
        const picked = pickProperties(
            this.threadInfo.value()!,
            'name',
            'allowDirectJoinWithoutInvitation',
            'allowMemberSoftInvitation',
            'allowMembersEnlistAllMembers',
            'allowMembersSendMessages',
            'allowSearchByName'
        );
        return {
            ...picked,
            iconFilePath: this.threadInfo.value()!.imagePath,
        };
    });

    constructor(
        private threadInfoCacheDictionary: ThreadInfoCacheDictionary,
        private threadsApiService: ThreadsApiService,
        private eventService: EventService
    ) {
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
    }

    public updateName(newName: string) {
        this.threadProfile.set({
            ...this.threadProfile(),
            name: newName,
        });
        this.saveProfile();
    }

    public saveProfile(quiet = false) {
        if (this.updatingSetting && !this.updatingSetting.closed) {
            this.updatingSetting.unsubscribe();
            this.updatingSetting = undefined;
        }

        this.updatingSetting = this.threadsApiService
            .UpdateThread(this.id(), this.threadProfile())
            .subscribe({
                next: () => {
                    this.updatingSetting = undefined;
                    if (!quiet) {
                        void SwalToast.fire('Saved!', '', 'success');
                    }
                    this.threadInfoCacheDictionary.delete(this.id());
                    void this.threadInfo.reload();
                },
                error: (err) => {
                    this.updatingSetting = undefined;
                    showCommonErrorDialog(err);
                }
            });
    }
}
