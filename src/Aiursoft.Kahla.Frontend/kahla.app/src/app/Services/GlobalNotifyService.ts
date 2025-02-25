import { Injectable } from '@angular/core';
import { EventService } from './EventService';
import { KahlaEvent } from '../Models/Events/KahlaEvent';
import { KahlaEventType } from '../Models/Events/EventType';

@Injectable({ providedIn: 'root' })
export class GlobalNotifyService {
    constructor(private eventService: EventService) {}

    private OnMessage(ev: KahlaEvent) {
        switch (ev.type) {
            case KahlaEventType.NewMessage: {
                break;
            }
            default:
                break;
        }
    }

    public init() {
        this.eventService.onMessage.subscribe(t => this.OnMessage(t));
    }
}
