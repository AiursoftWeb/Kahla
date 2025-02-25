import { ThreadInfoJoined } from '../Threads/ThreadInfo';
import { KahlaEventType } from './EventType';
import { KahlaEvent } from './KahlaEvent';

export interface ThreadAddedEvent extends KahlaEvent {
    type:
        | KahlaEventType.CreateScratched
        | KahlaEventType.YouDirectJoined
        | KahlaEventType.YourHardInviteFinished
        | KahlaEventType.YouWasHardInvited
        | KahlaEventType.YouCompletedSoftInvited;

    thread: ThreadInfoJoined;
}

export function isThreadAddedEvent(event: KahlaEvent): event is ThreadAddedEvent {
    return !!(event.type & 16);
}
