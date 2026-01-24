import { Component, computed, input } from '@angular/core';
import { ThreadInfo, ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { CacheService } from '../Services/CacheService';
import { ThreadMemberInfo } from '../Models/Threads/ThreadMemberInfo';

@Component({
    selector: 'app-thread-avatar',
    templateUrl: '../Views/thread-avatar.html',
    styleUrls: ['../Styles/thread-avatar.scss'],
    standalone: false,
})
export class ThreadAvatarComponent {
    public thread = input.required<ThreadInfo | ThreadInfoJoined>();

    constructor(private cacheService: CacheService) {}

    /**
     * Check if the imagePath is the special placeholder for dynamic avatars
     */
    public isSpecialIcon = computed(() => {
        return this.thread().imagePath === '{THE OTHER USER ICON}';
    });

    /**
     * Get the topTenMembers if available (only on ThreadInfoJoined)
     */
    private getMembers(): ThreadMemberInfo[] {
        const thread = this.thread() as ThreadInfoJoined;
        return thread.topTenMembers ?? [];
    }

    /**
     * Get the list of members to display as avatars (up to 9)
     * - 1 person: show self
     * - 2 people: show the other user (not self)
     * - 3+ people: show first 9 members
     */
    public avatarMembers = computed((): ThreadMemberInfo[] => {
        const members = this.getMembers();
        const me = this.cacheService.mine()?.me;

        if (members.length === 0) {
            return [];
        }

        if (members.length === 1) {
            // Only self, show self
            return members;
        }

        if (members.length === 2) {
            // Two people: show the other person (not me)
            if (me) {
                const other = members.filter(m => m.user.id !== me.id);
                return other.length > 0 ? other : members;
            }
            return members;
        }

        // 3+ people: return first 9 for the grid
        return members.slice(0, 9);
    });

    /**
     * Whether to show a grid layout (3+ members with special icon)
     */
    public showGrid = computed(() => {
        return this.isSpecialIcon() && this.avatarMembers().length >= 3;
    });

    /**
     * Get the single avatar URL to display (for 1-2 members or normal imagePath)
     */
    public singleAvatarPath = computed((): string => {
        if (!this.isSpecialIcon()) {
            return this.thread().imagePath;
        }

        const members = this.avatarMembers();
        if (members.length > 0) {
            return members[0].user.iconFilePath;
        }

        // Fallback to current user's avatar
        return this.cacheService.mine()?.me.iconFilePath ?? '';
    });
}
