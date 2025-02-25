import { Directive, ElementRef, model, OnInit } from '@angular/core';
import { MessageTextWithAnnotate } from '../Models/Messages/MessageSegments';
import { MessageTextAnnotatedMention } from '../Models/Messages/MessageTextAnnotated';
import { checkIfChildOf } from '../Utils/DomUtils';
import { KahlaUser } from '../Models/KahlaUser';
import { Subject } from 'rxjs';
import { Platform } from '@angular/cdk/platform';

@Directive({
    selector: '[appMessageTextInput]',
    host: {
        // Currently, only nightly version of firefox supports plaintext-only.
        contenteditable: 'plaintext-only',
        '(input)': 'oninput()',
        '(document:selectionchange)': 'selectionChanged()',
    },
    exportAs: 'appMessageTextInput',
})
export class MessageTextInputDirective implements OnInit {
    constructor(
        public elementRef: ElementRef<HTMLElement>,
        private platform: Platform
    ) {}

    ngOnInit(): void {
        this.forwardInternal();
    }

    /**
     * The textContent will not be render to the DOM directly,
     * since it will interrupt user input and reset caret position.
     *
     * To make the changes take effect, call forward() method.
     */
    textContent = model<MessageTextWithAnnotate[]>([]);

    // This subject will be emitted VERY FREQUENTLY, and it might emit multi times for a single input on some browser,
    // so make sure to piping into some distinct and throttle operators before subscribing.
    lastInputWordChanged = new Subject<{
        word?: string;
        caretEndPos: [number, number];
    }>();

    private caretInfo?: Range = undefined;

    public forward() {
        setTimeout(() => this.forwardInternal(), 0);
    }

    private forwardInternal() {
        // We cannot use angular component to render since angular relies on the assumption that the DOM is immutable outside angular.
        const container = this.elementRef.nativeElement;
        container.innerHTML = '';
        for (const item of this.textContent()) {
            if (typeof item !== 'string' && item.annotated === 'mention') {
                container.appendChild(
                    this.createMentionNode(
                        (item as MessageTextAnnotatedMention).targetId,
                        item.content
                    )
                );
            }
        }
    }

    backward() {
        let child = this.elementRef.nativeElement.firstChild;
        let results: MessageTextWithAnnotate[] = [];
        while (child) {
            this.backwardNodes(child, results);
            child = child.nextSibling;
        }
        results = results.filter(t => typeof t !== 'string' || t.trim());
        this.textContent.set(results);
    }

    private backwardNodes(node: ChildNode, results: MessageTextWithAnnotate[]) {
        function appendText(text: string, results: MessageTextWithAnnotate[]) {
            if (!text) return;
            if (results.length && typeof results[results.length - 1] === 'string') {
                (results[results.length - 1] as string) += text;
            } else {
                results.push(text);
            }
        }

        if (node.nodeType === Node.TEXT_NODE) {
            appendText(node.nodeValue!, results);
        } else if (node.nodeType === Node.ELEMENT_NODE) {
            const element = node as HTMLElement;
            if (element.hasAttribute('data-mention')) {
                const mentionId = element.getAttribute('data-mention-id');
                results.push({
                    annotated: 'mention',
                    targetId: mentionId!,
                    content: element.innerText,
                } satisfies MessageTextAnnotatedMention);
            } else if (element.tagName === 'BR') {
                appendText('\n', results);
            } else {
                let child = element.firstChild;
                while (child) {
                    this.backwardNodes(child, results);
                    child = child.nextSibling;
                }

                if (element.computedStyleMap().get('display')?.toString() === 'block') {
                    appendText('\n', results);
                }
            }
        }
    }

    private getCurrentCaret(): Range | null {
        const selection = window.getSelection();
        if (!selection?.rangeCount) return null;

        const range = selection.getRangeAt(0);
        // check if selection is in the current element
        if (
            range.startContainer &&
            checkIfChildOf(range.startContainer, this.elementRef.nativeElement)
        ) {
            return range;
        }

        return null;
    }

    insertTextToCaret(text: string) {
        this.insertNodeToCaret(document.createTextNode(text));
    }

    insertMentionToCaret({ id, nickName }: KahlaUser) {
        this.insertNodeToCaret(this.createMentionNode(id, `@${nickName}`));
    }

    insertNodeToCaret(node: Node) {
        const caret = this.getCurrentCaret() ?? this.caretInfo;
        if (!caret) {
            // just append to the end
            this.elementRef.nativeElement.appendChild(node);
            return;
        }

        caret.deleteContents();
        caret.insertNode(node);
        caret.collapse(false);

        this.backward();

        return caret;
    }

    private createMentionNode(id: string, content: string) {
        const mentionSpan = document.createElement('span');
        mentionSpan.contentEditable = 'false';
        mentionSpan.className = 'mention-tag';
        mentionSpan.setAttribute('data-mention', '');
        mentionSpan.setAttribute('data-mention-id', id);
        mentionSpan.textContent = content;
        return mentionSpan;
    }

    oninput() {
        // backward on every input might have performance impact, try optimizing if necessary.
        this.backward();

        // In chrome, backspace won't fire selectionchange event, but typing will, WTF!
        this.selectionChanged();

        // Ensure a newline at the end, I don't know why but it's necessary to make inserting line break works correctly.
        // This will make the result text having a trailing newline, but anyway, it doesn't matter.
        if (
            this.elementRef.nativeElement.lastChild?.nodeType !== Node.TEXT_NODE ||
            this.elementRef.nativeElement.lastChild?.nodeValue !== '\n'
        ) {
            this.elementRef.nativeElement.appendChild(document.createTextNode('\n'));
        }
    }

    selectionChanged() {
        const caret = this.getCurrentCaret();
        // logger.debug('caret changed', caret);
        if (!caret) return;
        this.caretInfo = caret.cloneRange();

        // Check the nearest inputting word
        if (caret.endContainer.nodeType !== Node.TEXT_NODE) return;
        const lastWord = caret.endContainer.textContent
            ?.slice(0, caret.endOffset)
            .split(' ')
            .at(-1);
        let { left, top } = caret.getBoundingClientRect();

        // In iOS, the bounding client rect's coordinate is relative to the visual viewport, not the entire window.
        // However, in Chromium, it relative to the entire window. WTF!
        if (this.platform.IOS && window.visualViewport) {
            left += window.visualViewport.offsetLeft;
            top += window.visualViewport.offsetTop;
        }
        this.lastInputWordChanged.next({
            word: lastWord,
            caretEndPos: [left, top],
        });
    }

    removeTextFromCursorTill(delim: string) {
        const caret = this.getCurrentCaret() ?? this.caretInfo;
        if (!caret) return;
        const text = caret.startContainer.textContent!;
        const start = text.lastIndexOf(delim, caret.startOffset - 1);
        if (start === -1) return;
        caret.setStart(caret.startContainer, start);
        caret.deleteContents();

        this.backward();
    }

    focus() {
        // Focus & set selection to last if not present
        if (!this.getCurrentCaret()) {
            const range = document.createRange();
            range.selectNodeContents(this.elementRef.nativeElement);
            range.collapse(false);
            const selection = window.getSelection();
            selection?.removeAllRanges();
            selection?.addRange(range);
        }

        this.elementRef.nativeElement.focus();
    }
}
