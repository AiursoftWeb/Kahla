import { Component, input, output } from '@angular/core';
import { CacheService } from '../Services/CacheService';
import { ParsedMessage } from '../Models/Messages/ParsedMessage';
import { KahlaUser } from '../Models/KahlaUser';

@Component({
    selector: 'app-message-list',
    templateUrl: '../Views/message-list.html',
    styleUrls: ['../Styles/message-list.scss'],
    standalone: false,
})
export class MessageListComponent {
    messages = input.required<ParsedMessage[]>();
    lastReadIndex = input<number>(-1);

    mention = output<KahlaUser>();

    constructor(public cacheService: CacheService) {}
}
