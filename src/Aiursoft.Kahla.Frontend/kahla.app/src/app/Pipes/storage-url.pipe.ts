import { Pipe, PipeTransform } from '@angular/core';
import { Values } from '../values';
@Pipe({
    name: 'storageUrl',
    standalone: true,
})
export class StorageUrlPipe implements PipeTransform {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    transform(value?: string): string {
        return Values.loadingImgURL; // TODO: Implement the new OSS
    }
}
