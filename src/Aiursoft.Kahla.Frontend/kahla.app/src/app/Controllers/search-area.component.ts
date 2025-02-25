import { Component, effect, EventEmitter, input, Input, model, Output } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounce, interval } from 'rxjs';

@Component({
    selector: 'app-search-area',
    templateUrl: '../Views/search-area.html',
    styleUrls: ['../Styles/search-area.scss', '../Styles/button.scss'],
    standalone: false,
})
export class SearchAreaComponent {
    searchText = model('');

    @Input() placeHolder = 'Search...';

    @Output() positiveClicked = new EventEmitter<void>();

    debounceTime = input(500);

    aggressive = input(true);

    textInput = new FormControl('');

    constructor() {
        effect(cleanup => {
            if (this.aggressive()) {
                const sub = this.textInput.valueChanges
                    .pipe(
                        debounce(t => {
                            return interval(t ? this.debounceTime() : 0);
                        })
                    )
                    .subscribe(term => {
                        this.searchText.set(term ?? '');
                    });
                cleanup(() => sub.unsubscribe());
            }
        });
    }

    onpositiveClicked() {
        this.searchText.set(this.textInput.value ?? '');
        this.positiveClicked.emit();
    }
}
