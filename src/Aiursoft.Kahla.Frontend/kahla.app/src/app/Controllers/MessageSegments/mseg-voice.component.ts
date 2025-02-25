import { Component, input } from '@angular/core';
import { MessageSegmentVoice } from '../../Models/Messages/MessageSegments';

@Component({
    selector: 'app-mseg-voice',
    templateUrl: '../../Views/MessageSegments/mseg-voice.html',
    styleUrls: ['../../Styles/MessageSegments/mseg-voice.scss'],
    standalone: false,
})
export class MessageSegmentVoiceComponent {
    content = input.required<MessageSegmentVoice>();
    audioPlaying = false;
}
