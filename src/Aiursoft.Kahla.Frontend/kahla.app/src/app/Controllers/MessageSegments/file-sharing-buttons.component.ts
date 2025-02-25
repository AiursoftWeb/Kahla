import { Component, input } from '@angular/core';
import { MessageSegmentFileLike } from '../../Models/Messages/MessageSegments';

@Component({
    selector: 'app-file-sharing-buttons',
    templateUrl: '../../Views/MessageSegments/file-sharing-buttons.html',
    styleUrls: ['../../Styles/MessageSegments/file-sharing-buttons.scss'],
    standalone: true,
})
export class FileSharingButtonsComponent {
    content = input.required<MessageSegmentFileLike>();
}
