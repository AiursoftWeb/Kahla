import { Descendant, Editor, Element, Node, Text, Transforms, Range } from 'slate';
import { MessageTextWithAnnotate } from '../../Models/Messages/MessageSegments';
import { MessageTextAnnotatedMention } from '../../Models/Messages/MessageTextAnnotated';
import {
    createEmptyParagraph,
    createMentionElement,
    CustomElement,
    isMentionElement,
    MentionElement,
    ParagraphElement,
} from './slate-types';

/**
 * Convert MessageTextWithAnnotate[] to Slate Descendant[]
 * 
 * Input: ["Hello ", { annotated: "mention", targetId: "uuid", content: "@John" }, " world"]
 * Output: [{ type: 'paragraph', children: [{ text: 'Hello ' }, mentionElement, { text: ' world' }] }]
 */
export function toSlateValue(content: MessageTextWithAnnotate[]): Descendant[] {
    if (!content || content.length === 0) {
        return [createEmptyParagraph()];
    }

    const children: Descendant[] = [];

    for (const item of content) {
        if (typeof item === 'string') {
            // Split by newlines to create separate paragraphs
            const lines = item.split('\n');
            for (let i = 0; i < lines.length; i++) {
                if (lines[i]) {
                    children.push({ text: lines[i] });
                }
                // Add a newline text node for line breaks within paragraphs
                if (i < lines.length - 1) {
                    children.push({ text: '\n' });
                }
            }
        } else if (item.annotated === 'mention') {
            const mention = item as MessageTextAnnotatedMention;
            children.push(createMentionElement(mention.targetId, mention.content));
        }
    }

    // Ensure there's at least one text node
    if (children.length === 0) {
        children.push({ text: '' });
    }

    // Wrap in a paragraph
    const paragraph: ParagraphElement = {
        type: 'paragraph',
        children,
    };

    return [paragraph];
}

/**
 * Convert Slate Descendant[] to MessageTextWithAnnotate[]
 * 
 * Traverses Slate nodes and extracts text and mention elements.
 */
export function fromSlateValue(nodes: Descendant[]): MessageTextWithAnnotate[] {
    const results: MessageTextWithAnnotate[] = [];

    function appendText(text: string) {
        if (!text) return;
        if (results.length > 0 && typeof results[results.length - 1] === 'string') {
            (results[results.length - 1] as string) += text;
        } else {
            results.push(text);
        }
    }

    function traverseNode(node: Descendant) {
        if (Text.isText(node)) {
            appendText(node.text);
        } else if (Element.isElement(node)) {
            if (isMentionElement(node)) {
                results.push({
                    annotated: 'mention',
                    targetId: node.targetId,
                    content: node.character,
                } satisfies MessageTextAnnotatedMention);
            } else {
                // Traverse children for paragraph and other elements
                for (const child of node.children) {
                    traverseNode(child as Descendant);
                }
            }
        }
    }

    for (const node of nodes) {
        traverseNode(node);
    }

    // Filter out empty strings
    return results.filter(t => typeof t !== 'string' || t.trim());
}

/**
 * Insert a mention element at the current cursor position
 */
export function insertMention(editor: Editor, targetId: string, character: string) {
    const mention = createMentionElement(targetId, character);
    Transforms.insertNodes(editor, mention);
    Transforms.move(editor);
}

/**
 * Delete text from cursor back to the specified delimiter (e.g., '@')
 */
export function deleteTextBackTo(editor: Editor, delimiter: string): boolean {
    const { selection } = editor;
    if (!selection || !Range.isCollapsed(selection)) return false;

    const [start] = Range.edges(selection);
    const beforeText = Editor.string(editor, {
        anchor: Editor.start(editor, []),
        focus: start,
    });

    const lastDelimIndex = beforeText.lastIndexOf(delimiter);
    if (lastDelimIndex === -1) return false;

    // Calculate how many characters to delete
    const deleteCount = beforeText.length - lastDelimIndex;

    Transforms.delete(editor, {
        distance: deleteCount,
        unit: 'character',
        reverse: true,
    });

    return true;
}

/**
 * Get the word being typed at cursor (for autocomplete)
 * Returns the text from the last space or @ symbol to the cursor
 */
export function getWordAtCursor(editor: Editor): { word: string; range: Range } | null {
    try {
        const { selection } = editor;
        if (!selection || !Range.isCollapsed(selection)) return null;

        const [start] = Range.edges(selection);
        
        // Get the current node at the path
        let node: Node;
        try {
            [node] = Editor.node(editor, start.path);
        } catch {
            return null;
        }
        
        if (!Text.isText(node)) return null;

        const text = node.text.slice(0, start.offset);
        
        // Find the last @ symbol or space
        let wordStart = 0;
        for (let i = text.length - 1; i >= 0; i--) {
            if (text[i] === ' ' || text[i] === '\n') {
                wordStart = i + 1;
                break;
            }
            if (text[i] === '@') {
                wordStart = i;
                break;
            }
        }

        const word = text.slice(wordStart);
        
        const range: Range = {
            anchor: { path: start.path, offset: wordStart },
            focus: start,
        };

        return { word, range };
    } catch {
        return null;
    }
}

/**
 * Check if the editor is empty
 */
export function isEditorEmpty(editor: Editor): boolean {
    const content = Editor.string(editor, []);
    return content.trim() === '';
}

/**
 * Clear the editor content
 */
export function clearEditor(editor: Editor) {
    Transforms.delete(editor, {
        at: {
            anchor: Editor.start(editor, []),
            focus: Editor.end(editor, []),
        },
    });
    
    // Ensure there's always a paragraph
    const children = editor.children;
    if (children.length === 0) {
        Transforms.insertNodes(editor, createEmptyParagraph());
    }
}

/**
 * Focus the editor and move cursor to end
 */
export function focusEditorEnd(editor: Editor) {
    const end = Editor.end(editor, []);
    Transforms.select(editor, end);
}

/**
 * Insert text at cursor position
 */
export function insertText(editor: Editor, text: string) {
    Transforms.insertText(editor, text);
}

/**
 * Get the cursor position for positioning autocomplete menu
 */
export function getCursorPosition(editor: Editor): { x: number; y: number } | null {
    const { selection } = editor;
    if (!selection) return null;

    const domSelection = window.getSelection();
    if (!domSelection || domSelection.rangeCount === 0) return null;

    const range = domSelection.getRangeAt(0);
    const rect = range.getBoundingClientRect();

    return { x: rect.left, y: rect.top };
}

/**
 * Custom editor plugin to make mentions void (inline non-editable) elements
 */
export function withMentions<T extends Editor>(editor: T): T {
    const { isInline, isVoid, markableVoid } = editor;

    editor.isInline = (element: CustomElement) => {
        return element.type === 'mention' ? true : isInline(element);
    };

    editor.isVoid = (element: CustomElement) => {
        return element.type === 'mention' ? true : isVoid(element);
    };

    editor.markableVoid = (element: CustomElement) => {
        return element.type === 'mention' || markableVoid(element);
    };

    return editor;
}
