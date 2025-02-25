import { Pipe, PipeTransform } from '@angular/core';
@Pipe({
    name: 'truncatedNum',
    standalone: true,
})
export class TruncatedNumPipe implements PipeTransform {
    transform(value: number): string {
        return value >= 100 ? '99+' : value.toString();
    }
}
