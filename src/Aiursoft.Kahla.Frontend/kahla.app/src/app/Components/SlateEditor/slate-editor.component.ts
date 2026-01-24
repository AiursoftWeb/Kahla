import {
    Component,
    ElementRef,
    EventEmitter,
    Input,
    OnDestroy,
    OnInit,
    Output,
    model,
} from '@angular/core';
import { createEditor, Descendant, Editor, Range, Transforms } from 'slate';
import { withHistory } from 'slate-history';
import { AngularEditor, withAngular } from 'slate-angular';
import { Platform } from '@angular/cdk/platform';

import { MessageTextWithAnnotate } from '../../Models/Messages/MessageSegments';
import { KahlaUser } from '../../Models/KahlaUser';
import {
    createMentionElement,
    CustomElement,
    INITIAL_EDITOR_VALUE,
    isMentionElement,
} from './slate-types';
import {
    clearEditor,
    focusEditorEnd,
    fromSlateValue,
    getCursorPosition,
    getWordAtCursor,
    insertText,
    toSlateValue,
    withMentions,
} from './slate-utils';
import { MentionElementComponent } from './mention-element.component';

/**
 * Slate-based rich text editor component for Kahla chat input.
 * Supports text input and @mentions with autocomplete.
 */
@Component({
    selector: 'app-slate-editor',
    template: `
        <slate-editable
            [editor]="editor"
            [(ngModel)]="editorValue"
            (ngModelChange)="onValueChange($event)"
            [renderElement]="renderElement"
            [placeholder]="placeholder"
            (keydown)="onKeyDown($event)"
            (focus)="onFocus.emit()"
            (blur)="onBlur.emit()"
        ></slate-editable>
    `,
    styles: [`
        :host {
            display: block;
            width: 100%;
        }
        :host ::ng-deep slate-editable,
        :host ::ng-deep .slate-editable-container {
            display: block;
            min-height: auto !important;
            padding: 0 !important;
            margin: 0 !important;
        }
        :host ::ng-deep [data-slate-editor] {
            outline: none;
            min-height: 20px;
        }
        :host ::ng-deep [data-slate-editor] > *,
        :host ::ng-deep [data-slate-editor] p,
        :host ::ng-deep [data-slate-editor] [data-slate-node="element"] {
            margin: 0 !important;
            padding: 0 !important;
            line-height: 1.5;
        }
        :host ::ng-deep [data-slate-placeholder] {
            color: #999;
            pointer-events: none;
            user-select: none;
        }
    `],
    standalone: false,
})
export class SlateEditorComponent implements OnInit, OnDestroy {
    /**
     * Two-way binding for the text content in MessageTextWithAnnotate[] format
     */
    textContent = model<MessageTextWithAnnotate[]>([]);

    /**
     * Placeholder text when editor is empty
     */
    @Input() placeholder = '';

    /**
     * Emits when a word starting with @ is being typed (for autocomplete)
     */
    @Output() lastInputWordChanged = new EventEmitter<{
        word?: string;
        caretEndPos: [number, number];
    }>();

    /**
     * Emits on focus
     */
    @Output() onFocus = new EventEmitter<void>();

    /**
     * Emits on blur
     */
    @Output() onBlur = new EventEmitter<void>();

    /**
     * Emits on keydown for parent to handle (e.g., Enter to send)
     */
    @Output() keydown = new EventEmitter<KeyboardEvent>();

    // The Slate editor instance
    editor: Editor;
    editorValue: Descendant[] = INITIAL_EDITOR_VALUE;

    private isInternalUpdate = false;
    private lastEmittedWord: string | undefined = undefined;

    constructor(
        private elementRef: ElementRef<HTMLElement>,
        private platform: Platform
    ) {
        // Create the editor with history and Angular bindings
        this.editor = withMentions(withHistory(withAngular(createEditor())));
    }

    ngOnInit(): void {
        // Initialize with current content
        this.editorValue = toSlateValue(this.textContent());
    }

    ngOnDestroy(): void {
        // Cleanup if needed
    }

    /**
     * Render function for custom elements
     */
    renderElement = (element: CustomElement) => {
        if (isMentionElement(element)) {
            return MentionElementComponent;
        }
        return null; // Use default rendering for paragraphs
    };

    /**
     * Handle editor value changes - only emit to parent, don't sync back
     */
    onValueChange(value: Descendant[]) {
        this.isInternalUpdate = true;
        const content = fromSlateValue(value);
        this.textContent.set(content);
        this.isInternalUpdate = false;

        // Check for @ autocomplete trigger
        this.checkForMentionTrigger();
    }

    /**
     * Handle keydown events
     */
    onKeyDown(event: KeyboardEvent) {
        this.keydown.emit(event);
    }

    /**
     * Check if user is typing a mention and emit for autocomplete
     */
    private checkForMentionTrigger() {
        try {
            const result = getWordAtCursor(this.editor);
            
            if (result?.word?.startsWith('@')) {
                const pos = getCursorPosition(this.editor);
                if (pos) {
                    let { x, y } = pos;
                    
                    // Handle iOS visual viewport offset
                    if (this.platform.IOS && window.visualViewport) {
                        x += window.visualViewport.offsetLeft;
                        y += window.visualViewport.offsetTop;
                    }
                    
                    const word = result.word;
                    if (word !== this.lastEmittedWord) {
                        this.lastEmittedWord = word;
                        this.lastInputWordChanged.emit({
                            word,
                            caretEndPos: [x, y],
                        });
                    }
                }
            } else {
                if (this.lastEmittedWord !== undefined) {
                    this.lastEmittedWord = undefined;
                    this.lastInputWordChanged.emit({
                        word: undefined,
                        caretEndPos: [0, 0],
                    });
                }
            }
        } catch (e) {
            // Ignore errors during mention trigger check
            console.warn('Error checking mention trigger:', e);
        }
    }

    /**
     * Insert plain text at cursor position
     */
    insertTextToCaret(text: string) {
        insertText(this.editor, text);
        AngularEditor.focus(this.editor);
    }

    /**
     * Insert a mention for a user at cursor position
     */
    insertMentionToCaret(user: KahlaUser) {
        const mention = createMentionElement(user.id, `@${user.nickName}`);
        Transforms.insertNodes(this.editor, mention);
        Transforms.move(this.editor);
        AngularEditor.focus(this.editor);
    }

    /**
     * Remove text from cursor back to the specified delimiter
     * Used to remove the "@searchTerm" when selecting a mention
     */
    removeTextFromCursorTill(delim: string) {
        const { selection } = this.editor;
        if (!selection || !Range.isCollapsed(selection)) return;

        const result = getWordAtCursor(this.editor);
        if (result?.word?.startsWith(delim)) {
            Transforms.delete(this.editor, { at: result.range });
        }
    }

    /**
     * Clear the editor content
     */
    clear() {
        clearEditor(this.editor);
        this.textContent.set([]);
    }

    /**
     * Focus the editor
     */
    focus() {
        AngularEditor.focus(this.editor);
        focusEditorEnd(this.editor);
    }

    /**
     * Forward method for compatibility - refreshes editor from textContent
     */
    forward() {
        this.editorValue = toSlateValue(this.textContent());
    }
}
