import { Component, input, OnInit } from '@angular/core';
import { MessageSegmentImage } from '../../Models/Messages/MessageSegments';
import { HomeService } from '../../Services/HomeService';

@Component({
    selector: 'app-mseg-img',
    templateUrl: '../../Views/MessageSegments/mseg-img.html',
    styleUrls: ['../../Styles/MessageSegments/mseg-img.scss'],
    standalone: false,
})
export class MessageSegmentImgComponent implements OnInit {
    content = input.required<MessageSegmentImage>();

    maxWidth = 0;

    constructor(private homeService: HomeService) {}

    ngOnInit(): void {
        this.maxWidth = this.homeService.imageMaxWidth; // Shadow the value to prevent re-requesting the thumbnails
    }
}
