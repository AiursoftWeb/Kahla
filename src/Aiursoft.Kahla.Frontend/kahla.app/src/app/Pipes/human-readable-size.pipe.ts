import { Pipe, PipeTransform } from '@angular/core';
import { humanReadableBytes } from '../Utils/StringUtils';
@Pipe({
    name: 'humanReadableSize',
    standalone: true,
})
export class HumanReadableSizePipe implements PipeTransform {
    transform(value: number): string {
        return humanReadableBytes(value);
    }
}
