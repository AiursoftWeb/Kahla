import { MessagePreview } from '../MessagePreview';
import { ThreadMemberInfo } from './ThreadMemberInfo';

export interface ThreadInfo {
    // public attributes (can access anytime)
    id: number;
    name: string;
    imagePath: string;
    ownerId: string;
    createTime: Date;
    imInIt: boolean;
    allowDirectJoinWithoutInvitation: boolean;
}

export interface ThreadInfoJoined extends ThreadInfo {
    // private attributes (can access after joined)
    messageContext: {
        unReadAmount: number;
        latestMessage: MessagePreview;
    };
    totalMessages: number;
    muted: boolean;
    topTenMembers: ThreadMemberInfo[];
    lastMessageTime: Date;
    imAdmin: boolean;
    imOwner: boolean;
    unreadAtMe: boolean;

    // options
    allowSearchByName: boolean;
    allowMembersSendMessages: boolean;
    allowMembersEnlistAllMembers: boolean;
    allowMemberSoftInvitation: boolean;
}
