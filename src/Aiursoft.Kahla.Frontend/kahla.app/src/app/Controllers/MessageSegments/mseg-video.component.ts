import { Component, input } from '@angular/core';
import { MessageSegmentVideo } from '../../Models/Messages/MessageSegments';
import { HomeService } from '../../Services/HomeService';
import { VjsPlayerComponent } from '../vjs-player.component';
import { FileSharingButtonsComponent } from './file-sharing-buttons.component';

@Component({
    selector: 'app-mseg-video',
    templateUrl: '../../Views/MessageSegments/mseg-video.html',
    standalone: true,
    imports: [VjsPlayerComponent, FileSharingButtonsComponent],
})
export class MessageSegmentVideoComponent {
    content = input.required<MessageSegmentVideo>();

    constructor(public homeService: HomeService) {}
}
