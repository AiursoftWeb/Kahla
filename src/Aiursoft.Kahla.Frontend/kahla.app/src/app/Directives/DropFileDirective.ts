import { computed, Directive, input, output } from '@angular/core';
import { useFileDropper } from '../Utils/useFileDropper';

@Directive({
    selector: '[appDropFile]',
    host: {
        '(drop)': 'eventHandlers().onDrop($event)',
        '(dragover)': 'eventHandlers().onDragOver($event)',
        '(paste)': 'eventHandlers().onPaste($event)',
    },
})
export class DropFileDirective {
    readonly dropAccept = input<string[]>(['*/*']);
    readonly droppedFiles = output<[File, string][]>();
    readonly incorrectType = output<void>();

    readonly eventHandlers = computed(() =>
        useFileDropper(
            this.dropAccept(),
            files => {
                this.droppedFiles.emit(files);
            },
            () => {
                this.incorrectType.emit();
            }
        )
    );
}
