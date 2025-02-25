import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class HomeService {
    public currentPage = 0;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public pwaHomeScreenPrompt: any;
    public pwaHomeScreenSuccess = false;

    public get wideScreenEnabled(): boolean {
        return window.innerWidth > 730 && window.innerHeight < window.innerWidth;
    }

    public get contentWrapper(): HTMLDivElement {
        return document.querySelector('#contentWrapper')!;
    }

    // Nullable
    public get floatingHomeWrapper(): HTMLDivElement {
        return document.querySelector('#homeWrapper')!;
    }

    public get imageMaxWidth(): number {
        return Math.floor((this.contentWrapper.clientWidth - 40) * 0.7 - 20 - 2);
    }

    public get videoHeight(): number {
        return Math.max(Math.floor(Math.min((this.imageMaxWidth * 9) / 21, 400)), 170);
    }
}
