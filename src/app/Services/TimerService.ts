import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';
import { ConversationApiService } from './Api/ConversationApiService';
import { Timers } from '../Models/Timers';

@Injectable({
    providedIn: 'root'
})
export class TimerService {
    public destructTime = 'off';
    public formerTimer = Timers.off;

    constructor(
        private conversationApiService: ConversationApiService) {}

    public setTimer(conversationId: number): void {
        Swal.fire({
            title: 'Set self-destruct timer',
            input: 'select',
            inputOptions: {
                5: '5 seconds',
                60: '1 minute',
                600: '10 minutes',
                3600: '1 hour',
                [3600 * 24]: '1 day',
                [3600 * 24 * 7]: '1 week',
                [3600 * 24 * 30]: '1 month',
                [3600 * 24 * 365]: '1 year',
                [Math.pow(2, 31) - 1]: 'off'
            },
            inputPlaceholder: 'Select one',
            showCancelButton: true
        }).then(selected => {
            if (selected.value && selected.value !== this.formerTimer) {
                this.conversationApiService.UpdateMessageLifeTime(conversationId, selected.value as number)
                    .subscribe(result => {
                        this.updateDestructTime(selected.value as number);
                        if (result.code !== 0) {
                            Swal.fire({
                                title: 'Error!',
                                icon: 'error',
                                text: result.message
                            });
                        }
                    });
            }
        });
    }

    public updateDestructTime(time: number): void {
        this.formerTimer = Timers[this.destructTime];
        this.destructTime = this.getDestructTime(time);
    }

    private getDestructTime(time: number): string {
        time = Number(time);
        return Timers[time] ? Timers[time] : 'off';
    }
}
