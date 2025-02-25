import { Component, effect, EventEmitter, input, Output } from '@angular/core';

function isNearBottom(element: HTMLElement | null, threshold = 100, reverse = false) {
    let scrollTop: number, clientHeight: number, scrollHeight: number;

    if (!element) {
        // if no selector is provided, use window
        scrollTop = window.scrollY || document.documentElement.scrollTop;
        clientHeight = window.innerHeight;
        scrollHeight = document.documentElement.scrollHeight;
    } else {
        scrollTop = element.scrollTop;
        clientHeight = element.clientHeight;
        scrollHeight = element.scrollHeight;
    }
    if (reverse) {
        return scrollTop <= threshold;
    } else {
        return scrollTop + clientHeight >= scrollHeight - threshold;
    }
}

@Component({
    selector: 'app-load-more-button',
    templateUrl: '../Views/load-more-button.html',
    styleUrls: ['../Styles/load-more-button.scss'],
    standalone: false,
})
export class LoadMoreButtonComponent {
    @Output() requestLoadMore = new EventEmitter<void>();

    private lastAutoLoadMoreTimestamp = 0;

    reversedDirection = input(false);
    autoLoad = input(false);
    scrollHostElement = input<HTMLElement | null>(null);
    disabled = input(false);
    loading = input(false);
    throttleTime = input(500);

    constructor() {
        effect(cleanup => {
            if (this.autoLoad()) {
                const scrollEventTarget: EventTarget = this.scrollHostElement()
                    ? this.scrollHostElement()!
                    : window;
                scrollEventTarget.addEventListener('scroll', this.scrollHandler);
                cleanup(() => scrollEventTarget.removeEventListener('scroll', this.scrollHandler));
            }
        });
    }

    private scrollHandler = () => {
        if (this.loading()) {
            return;
        }
        if (isNearBottom(this.scrollHostElement(), 100, this.reversedDirection())) {
            if (this.lastAutoLoadMoreTimestamp + this.throttleTime() < Date.now()) {
                this.loadMore();
            } else {
                setTimeout(
                    this.scrollHandler,
                    this.throttleTime() - (Date.now() - this.lastAutoLoadMoreTimestamp)
                );
            }
        }
    };

    public loadMore() {
        if (!this.loading() && !this.disabled()) {
            this.lastAutoLoadMoreTimestamp = Date.now();
            this.requestLoadMore.emit();
        }
    }
}
