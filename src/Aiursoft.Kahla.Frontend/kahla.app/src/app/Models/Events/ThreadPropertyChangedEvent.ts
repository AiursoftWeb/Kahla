import { KahlaEvent } from './KahlaEvent';
import { KahlaEventType } from './EventType';

export interface ThreadPropertyChangedEvent extends KahlaEvent {
    type: KahlaEventType.ThreadPropertyChanged;
    threadId: number;
    threadName: string;
    threadImagePath: string;
}

export function isThreadPropertyChangedEvent(event: KahlaEvent): event is ThreadPropertyChangedEvent {
    return event.type === KahlaEventType.ThreadPropertyChanged;
}
