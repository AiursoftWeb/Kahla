import { ApiService } from './ApiService';
import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/';
import { UploadFile } from '../../Models/Probe/UploadFile';
import { filter, map } from 'rxjs/operators';
import { AiurValue } from '../../Models/AiurValue';
import { FileTokenApiModel } from '../../Models/ApiModels/FileTokenApiModel';
import { ForwardMediaApiModel } from '../../Models/ApiModels/ForwardMediaApiModel';

@Injectable()
export class FilesApiService {
    private static serverPath = '/files';

    constructor(
        private apiService: ApiService,
        private http: HttpClient
    ) {}

    public InitIconUpload(): Observable<AiurValue<string>> {
        return this.apiService.Get(FilesApiService.serverPath + '/InitIconUpload');
    }

    public InitFileAccess(conversationId: number, upload = false): Observable<FileTokenApiModel> {
        return this.apiService.Get(
            `${FilesApiService.serverPath}/InitFileAccess` +
                `?ConversationId=${conversationId}&upload=${upload}&download=${!upload}`
        );
    }

    public ForwardMedia(
        sourceConversationId: number,
        sourceFilePath: string,
        targetConversationId: number
    ): Observable<ForwardMediaApiModel> {
        return this.apiService.Post(`${FilesApiService.serverPath}/ForwardMedia`, {
            SourceConversationId: sourceConversationId,
            SourceFilePath: sourceFilePath,
            TargetConversationId: targetConversationId,
        });
    }

    public UploadFile(formData: FormData, uploadURL: string): Observable<number | UploadFile> {
        const req = new HttpRequest('POST', uploadURL, formData, {
            reportProgress: true,
        });

        return this.http.request<UploadFile>(req).pipe(
            map(event => this.getProgress<UploadFile>(event)),
            filter(progress => progress !== null)
            // catchError(this.apiService.handleError)
        );
    }

    private getProgress<T>(event: HttpEvent<T>): number | T | null {
        switch (event.type) {
            case HttpEventType.UploadProgress:
                return event.total ? Math.round((100 * event.loaded) / event.total) : -1;
            case HttpEventType.Response:
                return event.body!;
            default:
                return null;
        }
    }
}
