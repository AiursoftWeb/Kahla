import { Component, Input, model } from '@angular/core';

export interface SearchTypeItem {
    className: string;
    title: string;
    showDot: boolean;
    dotValue?: number;
}

@Component({
    selector: 'app-search-type',
    templateUrl: '../Views/search-type.html',
    styleUrls: ['../Styles/search-type.scss', '../Styles/reddot.scss'],
    standalone: false,
})
export class SearchTypeComponent {
    currentIndex = model(0);

    @Input() items: SearchTypeItem[] = [];
}
