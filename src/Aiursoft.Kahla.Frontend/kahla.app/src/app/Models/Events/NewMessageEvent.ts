import { KahlaEvent } from './KahlaEvent';
import { MessagePreview } from '../MessagePreview';
import { KahlaEventType } from './EventType';

export interface NewMessageEvent extends KahlaEvent {
    type: KahlaEventType.NewMessage;
    message: MessagePreview;
    mentioned: boolean;
    threadName: string;
}
