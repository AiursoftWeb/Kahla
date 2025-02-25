import { AfterViewInit, Directive, ElementRef } from '@angular/core';
import { isMobileDevice } from '../Utils/EnvironmentUtils';

@Directive({
    selector: '[appAutofocus]',
})
export class AutofocusDirective implements AfterViewInit {
    constructor(private el: ElementRef<HTMLElement>) {}

    ngAfterViewInit() {
        // window.setTimeout(() => {
        if (!isMobileDevice()) {
            // For SSR (server side rendering) this is not safe. Use: https://github.com/angular/angular/issues/15008#issuecomment-285141070)
            this.el.nativeElement.focus();
        }
        // });
    }
}
