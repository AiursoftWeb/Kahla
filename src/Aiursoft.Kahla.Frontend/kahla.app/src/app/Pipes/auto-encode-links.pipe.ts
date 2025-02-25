import { Pipe, PipeTransform } from '@angular/core';
import Autolinker from 'autolinker';

@Pipe({
    name: 'autoEncodeLink',
})
export class AutoEncodeLinkPipe implements PipeTransform {
    transform(value: string, annotatedClassName?: string): string {
        return Autolinker.link(value, {
            sanitizeHtml: true,
            stripPrefix: false,
            className: annotatedClassName,
        });
    }
}
