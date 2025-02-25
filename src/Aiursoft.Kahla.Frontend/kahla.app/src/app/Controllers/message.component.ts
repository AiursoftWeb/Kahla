import { Component, effect, input, output } from '@angular/core';
import {
    MessageSegmentBase,
    MessageSegmentFile,
    MessageSegmentImage,
    MessageSegmentText,
    MessageSegmentVideo,
    MessageSegmentVoice,
} from '../Models/Messages/MessageSegments';
import { ParsedMessage } from '../Models/Messages/ParsedMessage';
import { UserInfoCacheDictionary } from '../Caching/UserInfoCacheDictionary';
import { KahlaUser } from '../Models/KahlaUser';

@Component({
    selector: 'app-message',
    templateUrl: '../Views/message.html',
    styleUrls: ['../Styles/message.scss', '../Styles/popups.scss'],
    host: {
        '[class.grouped]': 'groupWithPrevious()',
        '[class.left]': '!isByMe()',
        '[class.right]': 'isByMe()',
    },
    standalone: false,
})
export class MessageComponent {
    message = input.required<ParsedMessage>();
    isByMe = input<boolean>(false);
    groupWithPrevious = input<boolean>(false);
    isSending = input<boolean>(false);
    isFailed = input<boolean>(false);
    showNickNames = input<boolean>(false);

    mention = output<KahlaUser>();
    reply = output();

    userInfo?: KahlaUser;

    constructor(userInfoCacheDictionary: UserInfoCacheDictionary) {
        effect(async () => {
            this.userInfo = await userInfoCacheDictionary.get(this.message().senderId);
        });
    }

    public asTextSeg(seg: MessageSegmentBase): MessageSegmentText {
        if (seg.type !== 'text') {
            throw new Error('Expected text segment');
        }
        return seg as MessageSegmentText;
    }

    public asImageSeg(seg: MessageSegmentBase): MessageSegmentImage {
        if (seg.type !== 'image') {
            throw new Error('Expected image segment');
        }
        return seg as MessageSegmentImage;
    }

    public asVideoSeg(seg: MessageSegmentBase): MessageSegmentVideo {
        if (seg.type !== 'video') {
            throw new Error('Expected video segment');
        }
        return seg as MessageSegmentVideo;
    }

    public asVoiceSeg(seg: MessageSegmentBase): MessageSegmentVoice {
        if (seg.type !== 'voice') {
            throw new Error('Expected voice segment');
        }
        return seg as MessageSegmentVoice;
    }

    public asFileSeg(seg: MessageSegmentBase): MessageSegmentFile {
        if (seg.type !== 'file') {
            throw new Error('Expected file segment');
        }
        return seg as MessageSegmentFile;
    }
}
