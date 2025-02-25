import { KahlaEventType } from './EventType';
import { KahlaEvent } from './KahlaEvent';

export interface ThreadRemovedEvent extends KahlaEvent {
    type: KahlaEventType.ThreadDissolved | KahlaEventType.YouBeenKicked | KahlaEventType.YouLeft;

    threadId: number;
    threadName: string;
}

export function isThreadRemovedEvent(event: KahlaEvent): event is ThreadRemovedEvent {
    return !!(event.type & 8);
}
