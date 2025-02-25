import { KahlaEventType } from './EventType';
import { KahlaEvent } from './KahlaEvent';
import { NewMessageEvent } from './NewMessageEvent';
import { isThreadAddedEvent } from './ThreadAddedEvent';
import { isThreadRemovedEvent } from './ThreadRemovedEvent';

export function eventNotificationDescription(event: KahlaEvent): [string, string] {
    if (event.type === KahlaEventType.NewMessage) {
        const newMessageEvent = event as NewMessageEvent;
        const threadName = newMessageEvent.threadName.includes('{THE OTHER USER}')
            ? null
            : ` in ${newMessageEvent.threadName}`;
        return [
            `${newMessageEvent.mentioned ? '[Mentioned you]' : ''} ${newMessageEvent.message.sender.nickName}${threadName ?? ''}`,
            newMessageEvent.message.preview,
        ];
    } else if (isThreadAddedEvent(event)) {
        switch (event.type) {
            case KahlaEventType.CreateScratched:
                return ['You created a new thread', event.thread.name];
            case KahlaEventType.YouDirectJoined:
                return ['You joined a thread', event.thread.name];
            case KahlaEventType.YourHardInviteFinished:
                return ['You invited someone to a thread', event.thread.name];
            case KahlaEventType.YouWasHardInvited:
                return ['You were invited to a thread', event.thread.name];
            case KahlaEventType.YouCompletedSoftInvited:
                return ['You joined a thread', event.thread.name];
        }
    } else if (isThreadRemovedEvent(event)) {
        switch (event.type) {
            case KahlaEventType.ThreadDissolved:
                return ['Thread dissolved', event.threadName];
            case KahlaEventType.YouBeenKicked:
                return ['You were kicked from a thread', event.threadName];
            case KahlaEventType.YouLeft:
                return ['You left a thread', event.threadName];
        }
    }

    throw new Error('Unknown event type');
}

export function eventNotificationUrl(event: KahlaEvent): string {
    if (event.type === KahlaEventType.NewMessage) {
        const newMessageEvent = event as NewMessageEvent;
        return `/talking/${newMessageEvent.message.threadId}`;
    } else if (isThreadAddedEvent(event)) {
        return `/talking/${event.thread.id}`;
    } else if (isThreadRemovedEvent(event)) {
        return `/`;
    }

    throw new Error('Unknown event type');
}
