import { Directive, effect, ElementRef, input } from '@angular/core';

@Directive({
    selector: '[appMatchHeightFor]',
})
export class MatchHeightForDirective {
    readonly appMatchHeightFor = input.required<HTMLElement>();

    private observer = new ResizeObserver(() => {
        this.updateHeight();
    });

    constructor(private elementRef: ElementRef<HTMLElement>) {
        effect(cleanup => {
            this.updateHeight();
            this.observer.observe(this.appMatchHeightFor());
            cleanup(() => this.observer.disconnect());
        });
    }

    private updateHeight() {
        this.elementRef.nativeElement.style.height = this.appMatchHeightFor().clientHeight + 'px';
    }
}
