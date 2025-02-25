import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class UploadService {
    // constructor(
    //     private filesApiService: FilesApiService,
    //     private probeService: ProbeService
    // ) {}
    // public upload(
    //     file: File,
    //     conversationID: number,
    //     fileType: FileType
    // ): Promise<AiurValue<MessagePreview>> {
    //     if (!this.validateFileSize(file)) {
    //         void Swal.fire(
    //             'Error',
    //             'File size should larger than or equal to one bit and less then or equal to 2047MB.',
    //             'error'
    //         );
    //         return;
    //     }
    //     const formData = new FormData();
    //     formData.append('file', file);
    //     if (fileType === FileType.Image && !this.validImageType(file, false)) {
    //         void Swal.fire(
    //             'Try again',
    //             'Only support .png, .jpg, .jpeg, .svg, gif or .bmp file',
    //             'error'
    //         );
    //         return;
    //     }
    //     if (fileType === FileType.Video && !this.validVideoType(file)) {
    //         Swal.fire('Try again', 'Only support .mp4, .webm or .ogg file', 'error');
    //         return;
    //     }
    //     if (fileType === FileType.Audio) {
    //         const audioSrc = URL.createObjectURL(file);
    //         const audioHTMLString = `<audio controls src="${audioSrc}"></audio>`;
    //         Swal.fire({
    //             title: 'Are you sure to send this message?',
    //             html: audioHTMLString,
    //             icon: 'question',
    //             showCancelButton: true,
    //         }).then(result => {
    //             if (result.value) {
    //                 this.filesApiService
    //                     .InitFileAccess(conversationID, true)
    //                     .subscribe(response => {
    //                         if (response.code === 0) {
    //                             this.filesApiService
    //                                 .UploadFile(formData, response.uploadAddress)
    //                                 .subscribe(
    //                                     res => {
    //                                         this.buildFileRef(res, fileType, file)?.then(t => {
    //                                             this.encryptThenSend(t, conversationID);
    //                                         });
    //                                     },
    //                                     () => {
    //                                         Swal.close();
    //                                         Swal.fire('Error', 'Upload failed', 'error');
    //                                     }
    //                                 );
    //                         }
    //                     });
    //             }
    //             URL.revokeObjectURL(audioSrc);
    //         });
    //     } else {
    //         const alert = this.fireUploadingAlert(
    //             `Uploading your ${fileType === FileType.Image ? 'image' : fileType === FileType.Video ? 'video' : 'file'}...`
    //         );
    //         return new Promise<AiurValue<MessagePreview>>((resolve, reject) => {
    //             this.filesApiService.InitFileAccess(conversationID, true).subscribe(response => {
    //                 if (response.code === 0) {
    //                     const mission = this.filesApiService
    //                         .UploadFile(formData, response.uploadAddress)
    //                         .subscribe(
    //                             res => {
    //                                 if (Number(res)) {
    //                                     this.updateAlertProgress(Number(res));
    //                                 } else if (res) {
    //                                     Swal.close();
    //                                     this.buildFileRef(res, fileType, file)?.then(t => {
    //                                         this.encryptThenSend(t, conversationID).then(t_ => {
    //                                             resolve(t_);
    //                                         });
    //                                     });
    //                                 }
    //                             },
    //                             () => {
    //                                 Swal.close();
    //                                 Swal.fire('Error', 'Upload failed', 'error');
    //                                 reject();
    //                             }
    //                         );
    //                     alert.then(result => {
    //                         if (result.dismiss) {
    //                             mission.unsubscribe();
    //                             reject();
    //                         }
    //                     });
    //                 } else {
    //                     reject();
    //                 }
    //             });
    //         });
    //     }
    // }
    // private fireUploadingAlert(title: string): Promise<SweetAlertResult> {
    //     const result = Swal.fire({
    //         title: title,
    //         html: '<div id="progressText">0%</div><progress id="uploadProgress" max="100"></progress>',
    //         showCancelButton: true,
    //         showConfirmButton: false,
    //         allowOutsideClick: false,
    //     });
    //     Swal.showLoading();
    //     Swal.enableButtons();
    //     return result;
    // }
    // private updateAlertProgress(progress: number): void {
    //     (Swal.getHtmlContainer().querySelector('#uploadProgress') as HTMLProgressElement).value =
    //         progress;
    //     (Swal.getHtmlContainer().querySelector('#progressText') as HTMLDivElement).innerText =
    //         `${progress}%`;
    // }
    // public encryptThenSend(
    //     fileRef: MessageFileRef,
    //     conversationID: number
    // ): Promise<AiurValue<MessagePreview>> {
    //     if (!fileRef) {
    //         return null;
    //     }
    //     throw new Error('Method not implemented.');
    // }
    // private validateFileSize(file: File): boolean {
    //     if (file === null || file === undefined) {
    //         return false;
    //     }
    //     return file.size >= 0.125 && file.size <= 2146435072;
    // }
    // public uploadAvatar(user: KahlaUser, file: File): void {
    //     if (this.validImageType(file, true)) {
    //         const formData = new FormData();
    //         formData.append('image', file);
    //         const alert = this.fireUploadingAlert('Uploading your avatar...');
    //         this.filesApiService.InitIconUpload().subscribe(response => {
    //             if (response.code === 0) {
    //                 const mission = this.filesApiService
    //                     .UploadFile(formData, response.value)
    //                     .subscribe(res => {
    //                         if (Number(res)) {
    //                             this.updateAlertProgress(Number(res));
    //                         } else if (res != null && (res as UploadFile).code === 0) {
    //                             Swal.close();
    //                             user.iconFilePath = (res as UploadFile).filePath;
    //                         }
    //                     });
    //                 alert.then(result => {
    //                     if (result.dismiss) {
    //                         mission.unsubscribe();
    //                     }
    //                 });
    //             }
    //         });
    //     } else {
    //         Swal.fire('Try again', 'Only support .png, .jpg, .jpeg or .bmp file', 'error');
    //     }
    // }
    // public uploadGroupAvater(group: unknown, file: File): void {
    //     // if (this.validImageType(file, true)) {
    //     //     const formData = new FormData();
    //     //     formData.append('image', file);
    //     //     const alert = this.fireUploadingAlert('Uploading group avatar...');
    //     //     this.filesApiService.InitIconUpload().subscribe(response => {
    //     //         if (response.code === 0) {
    //     //             const mission = this.filesApiService
    //     //                 .UploadFile(formData, response.value)
    //     //                 .subscribe(res => {
    //     //                     if (Number(res)) {
    //     //                         this.updateAlertProgress(Number(res));
    //     //                     } else if (res != null && (res as UploadFile).code === 0) {
    //     //                         Swal.close();
    //     //                         group.groupImagePath = (res as UploadFile).filePath;
    //     //                         group.avatarURL = this.probeService.encodeProbeFileUrl(
    //     //                             group.groupImagePath
    //     //                         );
    //     //                     }
    //     //                 });
    //     //             alert.then(result => {
    //     //                 if (result.dismiss) {
    //     //                     mission.unsubscribe();
    //     //                 }
    //     //             });
    //     //         }
    //     //     });
    //     // } else {
    //     //     Swal.fire('Try again', 'Only support .png, .jpg, .jpeg or .bmp file', 'error');
    //     // }
    // }
    // public validImageType(file: File, avatar: boolean): boolean {
    //     const validAvatarTypes = ['png', 'jpg', 'bmp', 'jpeg'];
    //     const validChatTypes = ['png', 'jpg', 'bmp', 'jpeg', 'gif', 'svg'];
    //     const fileExtension = file.name.substring(file.name.lastIndexOf('.') + 1).toLowerCase();
    //     if (avatar) {
    //         return validAvatarTypes.includes(fileExtension);
    //     } else {
    //         return validChatTypes.includes(fileExtension);
    //     }
    // }
    // public validVideoType(file: File): boolean {
    //     // https://developer.mozilla.org/en-US/docs/Web/HTML/Supported_media_formats
    //     const validVideoType = ['webm', 'mp4', 'ogg'];
    //     const fileExtension = file.name.substring(file.name.lastIndexOf('.') + 1).toLowerCase();
    //     return validVideoType.includes(fileExtension);
    // }
    // public buildFileRef(
    //     response: number | UploadFile,
    //     fileType: FileType,
    //     file: File
    // ): Promise<MessageFileRef> {
    //     if (!response || Number(response) || (response as UploadFile).code !== 0) {
    //         return null;
    //     }
    //     return new Promise<MessageFileRef>(resolve => {
    //         const fileRef = new MessageFileRef();
    //         fileRef.filePath = (response as UploadFile).filePath;
    //         fileRef.fileType = fileType;
    //         fileRef.fileSize = humanReadableBytes((response as UploadFile).fileSize);
    //         fileRef.fileName = file.name;
    //         if (fileType === FileType.Image) {
    //             loadImage(
    //                 file,
    //                 (img, data) => {
    //                     let orientation = 0,
    //                         width = img.width,
    //                         height = img.height;
    //                     if (data.exif) {
    //                         orientation = data.exif.get('Orientation');
    //                         if (orientation >= 5 && orientation <= 8) {
    //                             [width, height] = [height, width];
    //                         }
    //                     }
    //                     fileRef.imgWidth = width;
    //                     fileRef.imgHeight = height;
    //                     resolve(fileRef);
    //                 },
    //                 { meta: true }
    //             );
    //         } else {
    //             resolve(fileRef);
    //         }
    //     });
    // }
    // public getFileDescriptionFromType(fileType: FileType): string {
    //     switch (fileType) {
    //         case FileType.Image:
    //             return 'Image';
    //         case FileType.Video:
    //             return 'Video';
    //         case FileType.File:
    //             return 'File';
    //         case FileType.Audio:
    //             return 'Audio';
    //     }
    //     return '';
    // }
}
