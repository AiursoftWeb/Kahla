import { Component, computed, input } from '@angular/core';
import { MessageSegmentText, MessageTextWithAnnotate } from '../../Models/Messages/MessageSegments';
import { checkSingleEmoji } from '../../Utils/StringUtils';
import { MessageTextAnnotatedMention } from '../../Models/Messages/MessageTextAnnotated';

@Component({
    selector: 'app-mseg-text',
    templateUrl: '../../Views/MessageSegments/mseg-text.html',
    styleUrls: ['../../Styles/MessageSegments/mseg-text.scss'],
    standalone: false,
})
export class MessageSegmentTextComponent {
    content = input.required<MessageSegmentText>();

    textSegments = computed<MessageTextWithAnnotate[]>(() => {
        if (typeof this.content().content === 'string') {
            return [this.content().content as string];
        } else {
            return this.content().content as MessageTextWithAnnotate[];
        }
    });

    singleEmoji = computed(
        () =>
            this.textSegments().length === 1 &&
            checkSingleEmoji(this.asPureText(this.textSegments()[0]))
    );

    asPureText(para: MessageTextWithAnnotate): string {
        return typeof para === 'string' ? para : para.content;
    }

    asMentionAnnotation(para: MessageTextWithAnnotate): MessageTextAnnotatedMention | null {
        return typeof para !== 'string' && para.annotated === 'mention'
            ? (para as MessageTextAnnotatedMention)
            : null;
    }
}
