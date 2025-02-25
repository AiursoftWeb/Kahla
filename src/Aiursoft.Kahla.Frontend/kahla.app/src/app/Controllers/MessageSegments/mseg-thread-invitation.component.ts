import { Component, computed, input, resource } from '@angular/core';
import { MessageSegmentThreadInvitation } from '../../Models/Messages/MessageSegments';
import { CacheService } from '../../Services/CacheService';
import { ThreadInfoPublicCacheDictionary } from '../../Caching/ThreadInfoPublicCacheDictionary';
import { ThreadsApiService } from '../../Services/Api/ThreadsApiService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../../Utils/CommonErrorDialog';
import { SwalToast } from '../../Utils/Toast';

@Component({
    selector: 'app-mseg-thread-invitation',
    templateUrl: '../../Views/MessageSegments/mseg-thread-invitation.html',
    styleUrls: ['../../Styles/MessageSegments/mseg-thread-invitation.scss'],
    standalone: false,
})
export class MessageSegmentThreadInvitationComponent {
    readonly content = input.required<MessageSegmentThreadInvitation>();
    readonly targetMe = computed(
        () => this.content().targetUserId === this.cacheService.mine()?.me.id
    );
    readonly expired = computed(() => this.content().validTo < new Date().getTime());
    readonly valid = computed(() => !this.expired() && this.targetMe);

    readonly threadInfo = resource({
        request: () => this.content().id,
        loader: ({ request: id }) => this.threadInfos.get(id),
    });

    constructor(
        private cacheService: CacheService,
        private threadInfos: ThreadInfoPublicCacheDictionary,
        private threadsApiService: ThreadsApiService
    ) {}

    public async accept() {
        if (!this.valid()) return;
        try {
            await lastValueFrom(this.threadsApiService.CompleteSoftInvite(this.content().token));
            void SwalToast.fire('Accepted invitation!');
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }
}
