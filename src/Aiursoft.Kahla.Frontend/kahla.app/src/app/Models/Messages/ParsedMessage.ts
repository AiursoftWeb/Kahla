import { MessageContent } from './MessageContent';
import { MessageSegmentText } from './MessageSegments';
import { ChatMessage, KahlaCommit } from '@aiursoft/kahla.sdk';

export class ParsedMessage {
    constructor(
        public id: string,
        public parsedContent: MessageContent,
        public senderId: string,
        public sendTime: Date
    ) {}

    public static fromCommit(commit: KahlaCommit<ChatMessage>): ParsedMessage {
        return new ParsedMessage(
            commit.id,
            ParsedMessage.parseContent(commit.item.content),
            commit.item.senderId!,
            new Date(commit.commitTime)
        );
    }

    private static parseContent(content: string): MessageContent {
        try {
            const json = JSON.parse(content) as MessageContent;
            if (json.v !== 1) {
                throw new Error('Unsupported version');
            }
            return json;
        } catch {
            // fallback to plaintext
            return {
                segments: [
                    {
                        type: 'text',
                        content: content,
                        ats: [],
                    } satisfies MessageSegmentText,
                ],
                v: 1,
            };
        }
    }
}
