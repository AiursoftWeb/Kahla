import { HttpErrorResponse } from '@angular/common/http';
import Swal from 'sweetalert2';
import { AiurCollection } from '../Models/AiurCollection';
import { logger } from '../Services/Logger';

export function showCommonErrorDialog(err: unknown): undefined {
    logger.error(err);
    if (err instanceof HttpErrorResponse) {
        if (err.status) {
            const error = err.error as AiurCollection<string>;
            if (error.code === -10) {
                void Swal.fire(error.message, error.items.join('\n'), 'error');
            } else {
                void Swal.fire(
                    Math.floor(err.status / 100) === 4 ? 'Client Error' : 'Server Error', // 4xx are client errors, 5xx are server errors
                    error.message,
                    'error'
                );
            }
        } else {
            void Swal.fire('Network error', err.message, 'error');
        }
    } else {
        // eslint-disable-next-line @typescript-eslint/no-base-to-string
        void Swal.fire('Unknown error', err!.toString(), 'error');
    }
}
