import { Component, input } from '@angular/core';
import { ThreadInfo } from '../Models/Threads/ThreadInfo';

@Component({
    selector: 'app-thread-avatar',
    templateUrl: '../Views/thread-avatar.html',
    standalone: false,
})
export class ThreadAvatarComponent {
    public thread = input.required<ThreadInfo>();
}
