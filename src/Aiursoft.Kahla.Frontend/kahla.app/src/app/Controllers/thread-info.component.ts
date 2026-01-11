import { Component, computed, input, resource } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';
import { ThreadInfoCacheDictionary } from '../Caching/ThreadInfoCacheDictionary';
import { SwalToast, YesNoDialog } from '../Utils/Toast';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { MappedRepository, StaticRepository } from '../Repositories/RepositoryBase';
import { ContactListItem } from './contact-list.component';
import { ThreadMemberInfo } from '../Models/Threads/ThreadMemberInfo';
import { EventService } from '../Services/EventService';
import { isThreadPropertyChangedEvent, ThreadPropertyChangedEvent } from '../Models/Events/ThreadPropertyChangedEvent';
import { effect } from '@angular/core';
import { filter } from 'rxjs';

@Component({
    templateUrl: '../Views/thread-info.html',
    styleUrls: ['../Styles/thread-info.scss', '../Styles/menu.scss'],
    standalone: false,
})
export class ThreadInfoComponent {
    id = input.required<number>();

    thread = resource({
        request: () => this.id(),
        loader: async ({ request }) => {
            try {
                return this.threadInfoCacheDictionary.get(request, true);
            } catch (err) {
                showCommonErrorDialog(err);
                return null;
            }
        },
    });

    topTenRepo = computed(() => {
        return new MappedRepository<ContactListItem, ThreadMemberInfo>(
            new StaticRepository(this.thread.value()?.topTenMembers ?? []),
            t => ({
                ...t,
                tags: [
                    ...(t.isOwner ? ([{ desc: 'Owner', type: 'owner' }] as const) : []),
                    ...(t.isAdmin ? ([{ desc: 'Admin', type: 'admin' }] as const) : []),
                ],
            })
        );
    });

    constructor(
        private threadsApiService: ThreadsApiService,
        private threadInfoCacheDictionary: ThreadInfoCacheDictionary,
        private router: Router,
        private eventService: EventService
    ) {
        effect(cleanup => {
            const threadId = this.id();
            const sub = this.eventService.onMessage
                .pipe(filter(t => isThreadPropertyChangedEvent(t)))
                .subscribe(t => {
                    const ev = t as ThreadPropertyChangedEvent;
                    if (ev.threadId === threadId) {
                        void this.thread.reload();
                    }
                });
            cleanup(() => sub.unsubscribe());
        });
    }

    public async setMute(value: boolean) {
        try {
            await lastValueFrom(this.threadsApiService.SetMute(this.thread.value()!.id, value));
            this.threadInfoCacheDictionary.set(this.id(), {
                ...this.thread.value()!,
                muted: value,
            });
            this.thread.reload();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    async leaveThread() {
        const threadValue = this.thread.value()!;
        const isOwner = threadValue.imOwner;
        const memberCount = threadValue.topTenMembers.length;
        const threadId = threadValue.id;

        // If I am owner:
        // 1 user: dissolve
        // 2 users: transfer ownership + leave
        // >2 users: error

        if (isOwner && memberCount > 2) {
            void Swal.fire({
                icon: 'error',
                title: 'You are not allowed to leave this thread.',
                text: 'You are the owner of the thread. Consider transferring the ownership to others or dissolve the thread.',
            });
            return;
        }

        const confirmResult = await YesNoDialog.fire({
            title: isOwner && memberCount === 1 ? 'Dissolve this thread?' : 'Leave this thread?',
            text: 'You will not be able to undo this action.',
        });

        if (confirmResult.isDismissed) {
            return;
        }

        if (isOwner) {
            if (memberCount === 1) {
                try {
                    await lastValueFrom(this.threadsApiService.Dissolve(threadId));
                    this.threadInfoCacheDictionary.delete(this.id());
                    void SwalToast.fire({
                        icon: 'success',
                        title: 'Dissolved successfully.',
                    });
                    void this.router.navigate(['/home'], { replaceUrl: true });
                    return;
                } catch (err) {
                    showCommonErrorDialog(err);
                    return;
                }
            } else if (memberCount === 2) {
                const target = threadValue.topTenMembers.find(
                    m => m.user.id !== threadValue.ownerId
                );
                try {
                    await lastValueFrom(this.threadsApiService.Transfer(threadId, target!.user.id));
                } catch (err) {
                    showCommonErrorDialog(err);
                    return;
                }
            }
        }

        try {
            await lastValueFrom(this.threadsApiService.Leave(threadId));
            this.threadInfoCacheDictionary.delete(this.id());
            void SwalToast.fire({
                icon: 'success',
                title: 'Left successfully.',
            });
            void this.router.navigate(['/home'], { replaceUrl: true });
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }
}
