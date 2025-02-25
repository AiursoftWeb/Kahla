import { MessageSegmentBase } from './MessageSegments';

export interface MessageContent {
    segments: MessageSegmentBase[];
    v: number;
}
