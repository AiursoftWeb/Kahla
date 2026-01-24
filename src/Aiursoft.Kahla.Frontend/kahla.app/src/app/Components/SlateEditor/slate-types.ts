import { BaseEditor, Descendant } from 'slate';
import { AngularEditor } from 'slate-angular';
import { HistoryEditor } from 'slate-history';

/**
 * Custom Slate element types for Kahla chat input
 */

// Mention inline void element
export interface MentionElement {
    type: 'mention';
    targetId: string; // UUID of the mentioned user
    character: string; // Display text like "@John"
    children: [{ text: '' }]; // Void elements must have empty text children
}

// Paragraph block element
export interface ParagraphElement {
    type: 'paragraph';
    children: Descendant[];
}

// Custom text node
export interface CustomText {
    text: string;
}

// Union of all custom element types
export type CustomElement = MentionElement | ParagraphElement;

// Extend Slate's type declarations
declare module 'slate' {
    interface CustomTypes {
        Editor: BaseEditor & AngularEditor & HistoryEditor;
        Element: CustomElement;
        Text: CustomText;
    }
}

// Type guards
export function isMentionElement(element: unknown): element is MentionElement {
    return (
        typeof element === 'object' &&
        element !== null &&
        'type' in element &&
        (element as { type: string }).type === 'mention'
    );
}

export function isParagraphElement(element: unknown): element is ParagraphElement {
    return (
        typeof element === 'object' &&
        element !== null &&
        'type' in element &&
        (element as { type: string }).type === 'paragraph'
    );
}

// Helper to create a mention element
export function createMentionElement(targetId: string, character: string): MentionElement {
    return {
        type: 'mention',
        targetId,
        character,
        children: [{ text: '' }],
    };
}

// Helper to create an empty paragraph
export function createEmptyParagraph(): ParagraphElement {
    return {
        type: 'paragraph',
        children: [{ text: '' }],
    };
}

// Initial empty editor value
export const INITIAL_EDITOR_VALUE: Descendant[] = [createEmptyParagraph()];
