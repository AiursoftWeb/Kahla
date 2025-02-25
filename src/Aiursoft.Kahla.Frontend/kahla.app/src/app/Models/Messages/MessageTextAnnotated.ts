export type MessageTextAnnotatedTypes = MessageTextAnnotatedMention['annotated'];

export interface MessageTextAnnotated {
    annotated: MessageTextAnnotatedTypes;
    content: string;

    [otherOptions: string]: unknown;
}

export interface MessageTextAnnotatedMention extends MessageTextAnnotated {
    annotated: 'mention';
    targetId: string; // UUID
}

export function deAnnotationText(text: MessageTextAnnotated | string): string {
    if (typeof text === 'string') {
        return text;
    }
    return text.content;
}
