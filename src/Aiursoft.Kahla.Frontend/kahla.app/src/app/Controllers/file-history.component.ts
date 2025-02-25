import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FileHistoryApiModel } from '../Models/ApiModels/FileHistoryApiModel';
import { ProbeFile } from '../Models/Probe/ProbeFile';
import { ProbeService } from '../Services/ProbeService';
import { FilesApiService } from '../Services/Api/FilesApiService';
import { MessageService } from '../Services/MessageService';
import { humanReadableBytes } from '../Utils/StringUtils';

@Component({
    selector: 'app-file-history',
    templateUrl: '../Views/file-history.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/file-list.scss', '../Styles/button.scss'],
    standalone: false,
})
export class FileHistoryComponent implements OnInit {
    public files: FileHistoryApiModel[] = [];
    public conversationId: number;
    public currentSkip = 0;
    public accessToken = '';
    public loading: boolean;
    public noMoreFiles: boolean;

    constructor(
        private route: ActivatedRoute,
        private filesApiService: FilesApiService,
        private messageService: MessageService,
        public probeService: ProbeService,
        public router: Router
    ) {}

    ngOnInit(): void {
        this.route.params.subscribe(param => {
            this.conversationId = param.id;
            // get access token first
            this.loading = true;
            this.filesApiService.InitFileAccess(this.conversationId, false).subscribe(t => {
                this.accessToken = t.value;
                this.loadFiles();
            });
        });
    }

    public isImage(fileName: string): boolean {
        return !!fileName
            .substring(fileName.lastIndexOf('.') + 1)
            .match(/png|jpg|jpeg|gif|webp|bmp/);
    }

    public buildProbeUrl(file: ProbeFile, dir: FileHistoryApiModel, download = false): string {
        return this.probeService.encodeProbeFileUrl(
            `${dir.siteName}/${dir.rootPath}/${dir.showingDateUTC}/${file.fileName}`,
            this.accessToken,
            download
        );
    }

    public calcSummary() {
        let totalSize = 0;
        let count = 0;
        this.files.forEach(t => {
            count += t.items.length;
            t.items.forEach(t1 => {
                totalSize += t1.fileSize;
            });
        });
        return `Showing ${count} files. Total Size:${humanReadableBytes(totalSize)}`;
    }

    public share(file: ProbeFile, dir: FileHistoryApiModel) {
        // this.messageService.shareRef = {
        //     filePath: `${dir.showingDateUTC}/${file.fileName}`,
        //     fileType: FileType.File,
        //     fileName: file.fileName,
        //     fileSize: humanReadableBytes(file.fileSize),
        // } as MessageFileRef;
        this.router.navigate([
            'share-target',
            {
                srcConversation: this.conversationId,
                relativePath: true,
            },
        ]);
    }

    public loadFiles() {
        if (this.noMoreFiles) {
            return;
        }
        this.loading = true;
        // this.conversationApiService
        //     .FileHistory(this.conversationId, this.currentSkip++)
        //     .subscribe(t => {
        //         this.loading = false;
        //         if (t.code === 0) {
        //             this.files.push(t);
        //         } else if (t.code === -3) {
        //             this.noMoreFiles = true;
        //         }
        //     });
    }
}
