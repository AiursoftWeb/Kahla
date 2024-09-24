﻿import { Message } from './Message';

export class ContactInfo {
    public displayName: string;
    public displayImagePath: string;
    public latestMessage: Message;
    public unReadAmount: number;
    public conversationId: number;
    public discriminator: 'GroupConversation' | 'PrivateConversation';
    public userId: string;
    public avatarURL: string;
    public muted: boolean;
    public someoneAtMe: boolean;
    public online?: boolean;
}
