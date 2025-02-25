import { Component, input } from '@angular/core';
import { ThreadInfo } from '../Models/Threads/ThreadInfo';

@Component({
    selector: 'app-thread-basic-info',
    templateUrl: '../Views/thread-basic-info.html',
    styleUrls: ['../Styles/menu.scss'],
    standalone: false,
})
export class ThreadBasicInfoComponent {
    threadInfo = input.required<ThreadInfo>();
}
