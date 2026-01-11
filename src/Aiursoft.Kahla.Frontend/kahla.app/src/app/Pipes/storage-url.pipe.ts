import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

@Pipe({
    name: 'storageUrl',
    standalone: true,
})
export class StorageUrlPipe implements PipeTransform {
    transform(value?: string): string {
        if (!value) {
            return '';
        }
        if (value.startsWith('http')) {
            return value;
        }
        return `${environment.serversProvider}/files/open/${value}`;
    }
}
