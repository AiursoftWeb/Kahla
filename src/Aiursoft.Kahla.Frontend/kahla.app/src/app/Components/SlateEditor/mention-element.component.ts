import { Component, ElementRef, Renderer2 } from '@angular/core';
import { BaseElementComponent } from 'slate-angular';
import { MentionElement } from './slate-types';

/**
 * Component for rendering mention elements in the Slate editor.
 * Mentions are inline void elements that display as non-editable tags.
 */
@Component({
    selector: 'span[kahla-mention]',
    template: `<span contenteditable="false">{{ element.character }}</span>`,
    styles: [`
        :host {
            display: inline-block;
            padding: 2px 6px;
            margin: 0 2px;
            vertical-align: baseline;
            border-radius: 4px;
            background-color: var(--primary-color-depth1, rgba(0, 120, 215, 0.15));
            color: var(--primary-color-depth3, #0078d7);
            font-size: 0.95em;
            user-select: none;
            cursor: default;
        }
        :host:hover {
            background-color: var(--primary-color-depth2, rgba(0, 120, 215, 0.25));
        }
        span {
            pointer-events: none;
        }
    `],
    standalone: false,
})
export class MentionElementComponent extends BaseElementComponent<MentionElement> {
    constructor(
        public override elementRef: ElementRef,
        private renderer: Renderer2
    ) {
        super();
    }

    override onContextChange() {
        super.onContextChange();
        // Set data attributes for potential styling or identification
        if (this.element) {
            this.renderer.setAttribute(
                this.elementRef.nativeElement,
                'data-mention-id',
                this.element.targetId
            );
            this.renderer.setAttribute(
                this.elementRef.nativeElement,
                'data-slate-void',
                'true'
            );
        }
    }
}
