// vjs-player.component.ts
import {
    Component,
    ElementRef,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
    viewChild,
    input,
} from '@angular/core';
import videojs from 'video.js';

@Component({
    selector: 'app-vjs-player',
    template: ` <video
        #target
        class="video-js vjs-theme-sea"
        controls
        playsinline
        preload="metadata"
    ></video>`,
    styleUrls: ['../Styles/vjs-module.scss'],
    encapsulation: ViewEncapsulation.None,
    standalone: true,
})
export class VjsPlayerComponent implements OnInit, OnDestroy {
    readonly target = viewChild.required<ElementRef<HTMLVideoElement>>('target');
    // see options: https://github.com/videojs/video.js/blob/master/docs/guides/options.md
    readonly options = input<Partial<VjsPluginOptions>>({});
    player: ReturnType<typeof videojs>; // workaround to get the type of the player (https://github.com/videojs/video.js/issues/8242)

    ngOnInit() {
        // instantiate Video.js
        this.player = videojs(this.target().nativeElement, this.options());
    }

    ngOnDestroy() {
        // destroy player
        if (this.player) {
            this.player.dispose();
        }
    }
}

export interface VjsPluginOptions {
    fluid: boolean;
    aspectRatio: boolean;
    autoplay: boolean;
    controls: boolean;
    width: string | number;
    height: string | number;
    sources: {
        src: string;
        type?: string;
    }[];
}
