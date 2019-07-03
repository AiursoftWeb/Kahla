import { Injectable } from '@angular/core';
import { AES } from 'crypto-js';
import { FilesApiService } from './FilesApiService';
import Swal, { SweetAlertResult } from 'sweetalert2';
import { UploadFile } from '../Models/UploadFile';
import { KahlaUser } from '../Models/KahlaUser';
import { ConversationApiService } from './ConversationApiService';
import * as loadImage from 'blueimp-load-image';
import { GroupConversation } from '../Models/GroupConversation';
import { Values } from '../values';
import { HomeService } from './HomeService';

@Injectable({
    providedIn: 'root'
})
export class UploadService {
    public talkingDestroyed = false;

    constructor(
        private filesApiService: FilesApiService,
        private conversationApiService: ConversationApiService,
        private homeService: HomeService,
    ) {}

    public upload(file: File, conversationID: number, aesKey: string, fileType: number): void {
        if (!this.validateFileSize(file)) {
            Swal.fire('Error', 'File size should larger than or equal to one bit and less then or equal to 1000MB.', 'error');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        if (fileType === 0 && !this.validImageType(file, false)) {
            Swal.fire('Try again', 'Only support .png, .jpg, .jpeg, .svg, gif or .bmp file', 'error');
            return;
        }
        if (fileType === 0 || fileType === 1) {
            const alert = this.fireUploadingAlert(`Uploading your ${fileType === 0 ? 'Image' : 'Video'}...`);
            const mission = this.filesApiService.UploadMedia(formData).subscribe(response => {
                if (Number(response)) {
                    this.getAlertProgressBar().value = Number(response);
                } else if (response) {
                    // Done!
                    Swal.close();
                    this.encryptThenSend(response, fileType, conversationID, aesKey, file);
                }
            }, () => {
                Swal.close();
                Swal.fire('Error', 'Upload failed', 'error');
            });
            alert.then(result => {
                if (result.dismiss) {
                    mission.unsubscribe();
                }
            });
        } else if (fileType === 3) {
            const audioSrc = URL.createObjectURL(file);
            const audioHTMLString = `<audio controls src="${audioSrc}"></audio>`;
            Swal.fire({
                title: 'Are you sure to send this message?',
                html: audioHTMLString,
                type: 'question',
                showCancelButton: true
            }).then(result => {
                if (result.value) {
                    this.filesApiService.UploadFile(formData, conversationID).subscribe(response => {
                        this.encryptThenSend(response, fileType, conversationID, aesKey, file);
                    }, () => {
                        Swal.close();
                        Swal.fire('Error', 'Upload failed', 'error');
                    });
                }
                URL.revokeObjectURL(audioSrc);
            });
        } else {
            const alert = this.fireUploadingAlert('Uploading your file...');
            const mission = this.filesApiService.UploadFile(formData, conversationID).subscribe(response => {
                if (Number(response)) {
                    this.getAlertProgressBar().value = Number(response);
                } else if (response) {
                    Swal.close();
                    this.encryptThenSend(response, fileType, conversationID, aesKey, file);
                }
            }, () => {
                Swal.close();
                Swal.fire('Error', 'Upload failed', 'error');
            });
            alert.then(result => {
                if (result.dismiss) {
                    mission.unsubscribe();
                }
            });
        }
    }

    private fireUploadingAlert(title: string): Promise<SweetAlertResult> {
        const result = Swal.fire({
            title: title,
            html: `<progress id="uploadProgress" max="100"></progress>`,
            showCancelButton: true,
            showConfirmButton: false,
        });
        Swal.showLoading();
        Swal.enableButtons();
        return result;
    }

    private getAlertProgressBar(): HTMLProgressElement {
        return Swal.getContent().querySelector('#uploadProgress');
    }

    private encryptThenSend(response: number | UploadFile, fileType: number, conversationID: number, aesKey: string, file: File): void {
        if (response && !Number(response)) {
            if ((<UploadFile>response).code === 0) {
                let encedMessages;
                switch (fileType) {
                    case 0:
                        loadImage(
                            file,
                            function (img, data) {
                                let orientation = 0, width = img.width, height = img.height;
                                if (data.exif) {
                                    orientation = data.exif.get('Orientation');
                                    if (orientation >= 5 && orientation <= 8) {
                                        [width, height] = [height, width];
                                    }
                                }
                                encedMessages = AES.encrypt(`[img]${(<UploadFile>response).fileKey}-${width}-${
                                    height}-${orientation}`, aesKey).toString();
                                this.sendMessage(encedMessages, conversationID);
                            }.bind(this),
                            {meta: true}
                        );
                        break;
                    case 1:
                        encedMessages = AES.encrypt(`[video]${(<UploadFile>response).fileKey}`, aesKey).toString();
                        this.sendMessage(encedMessages, conversationID);
                        break;
                    case 2:
                        encedMessages = AES.encrypt(this.formatFileMessage(<UploadFile>response), aesKey).toString();
                        this.sendMessage(encedMessages, conversationID);
                        break;
                    case 3:
                        encedMessages = AES.encrypt(`[audio]${(<UploadFile>response).fileKey}`, aesKey).toString();
                        this.sendMessage(encedMessages, conversationID);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private sendMessage(message: string, conversationID: number): void {
        this.conversationApiService.SendMessage(conversationID, message, [])
            .subscribe(() => {
                this.scrollBottom(true);
            }, () => {
                const unsentMessages = new Map(JSON.parse(localStorage.getItem('unsentMessages')));
                if (unsentMessages.get(conversationID) && (<Array<string>>unsentMessages.get(conversationID)).length > 0) {
                    const tempArray = <Array<string>>unsentMessages.get(conversationID);
                    tempArray.push(message);
                    unsentMessages.set(conversationID, tempArray);
                } else {
                    unsentMessages.set(conversationID, [message]);
                }
                localStorage.setItem('unsentMessages', JSON.stringify(Array.from(unsentMessages)));
            });
    }

    private validateFileSize(file: File): boolean {
        if (file === null || file === undefined) {
            return false;
        }
        return file.size >= 0.125 && file.size <= 1000000000;
    }

    public scrollBottom(smooth: boolean): void {
        if (!this.talkingDestroyed) {
            const h = this.homeService.contentWrapper.scrollHeight;
            if (document.querySelector('.message-list').scrollHeight < window.innerHeight - 50) {
                this.homeService.contentWrapper.scroll(0, 0);
            } else if (smooth) {
                this.homeService.contentWrapper.scroll({top: h, behavior: 'smooth'});
            } else {
                this.homeService.contentWrapper.scroll(0, h);
            }
        }
    }

    public uploadAvatar(user: KahlaUser, file: File): void {
        if (this.validImageType(file, true)) {
            const formData = new FormData();
            formData.append('image', file);
            const alert = this.fireUploadingAlert('Uploading your avatar...');
            const mission = this.filesApiService.UploadIcon(formData).subscribe(response => {
                if (Number(response)) {
                    this.getAlertProgressBar().value = Number(response);
                } else if (response != null && (<UploadFile>response).code === 0) {
                    Swal.close();
                    user.headImgFileKey = (<UploadFile>response).fileKey;
                    user.avatarURL = (<UploadFile>response).downloadPath;
                }
            });
            alert.then(result => {
                if (result.dismiss) {
                    mission.unsubscribe();
                }
            });
        } else {
            Swal.fire('Try again', 'Only support .png, .jpg, .jpeg or .bmp file', 'error');
        }
    }

    public uploadGroupAvater(group: GroupConversation, file: File): void {
        if (this.validImageType(file, true)) {
            const formData = new FormData();
            formData.append('image', file);
            const alert = this.fireUploadingAlert('Uploading group avatar...');
            const mission = this.filesApiService.UploadIcon(formData).subscribe(response => {
                if (Number(response)) {
                    this.getAlertProgressBar().value = Number(response);
                } else if (response != null && (<UploadFile>response).code === 0) {
                    Swal.close();
                    group.groupImageKey = (<UploadFile>response).fileKey;
                    group.avatarURL = Values.fileAddress + group.groupImageKey;
                }
            });
            alert.then(result => {
                if (result.dismiss) {
                    mission.unsubscribe();
                }
            });
        } else {
            Swal.fire('Try again', 'Only support .png, .jpg, .jpeg or .bmp file', 'error');
        }
    }

    public validImageType(file: File, avatar: boolean): boolean {
        const validAvatarTypes = ['png', 'jpg', 'bmp', 'jpeg'];
        const validChatTypes = ['png', 'jpg', 'bmp', 'jpeg', 'gif', 'svg'];
        const fileExtension = file.name.substring(file.name.lastIndexOf('.') + 1).toLowerCase();
        if (avatar) {
            return validAvatarTypes.includes(fileExtension);
        } else {
            return validChatTypes.includes(fileExtension);
        }
    }

    public getFileKey(message: string): number {
        if (message === null || message.length < 5) {
            return -1;
        }
        if (message.startsWith('[img]')) {
            return Number(message.substring(5, message.indexOf('-')));
        } else if (message.startsWith('[file]')) {
            return Number(message.substring(6, message.indexOf('-')));
        } else if (message.startsWith('[video]') || message.startsWith('[audio]')) {
            return Number(message.substring(7));
        } else {
            return -1;
        }
    }

    public getFileURL(event: MouseEvent, message: string): void {
        event.preventDefault();
        const filekey = this.getFileKey(message);
        if (filekey !== -1 && !isNaN(filekey) && filekey !== 0) {
            this.filesApiService.GetFileURL(filekey).subscribe(response => {
                if (response.code === 0) {
                    window.location.href = response.downloadPath;
                }
            });
        }
    }

    public getAudio(target: HTMLElement, message: string): void {
        const filekey = this.getFileKey(message);
        if (filekey !== -1 && !isNaN(filekey) && filekey !== 0) {
            this.filesApiService.GetFileURL(filekey).subscribe(response => {
                if (response.code === 0) {
                    target.style.display = 'none';
                    const audioElement = document.createElement('audio');
                    audioElement.style.maxWidth = '100%';
                    audioElement.src = response.downloadPath;
                    audioElement.controls = true;
                    target.parentElement.appendChild(audioElement);
                    audioElement.play();
                }
            });
        }
    }

    private formatFileMessage(response: UploadFile): string {
        let message = '[file]';
        const units = ['kB', 'MB', 'GB'];
        const thresh = 1000;
        message += response.fileKey + '-';
        message += response.savedFileName.replace(/-/g, '') + '-';
        if (response.fileSize < thresh) {
            message += response.fileSize + ' B';
        } else {
            let index = -1;
            do {
                response.fileSize /= thresh;
                index++;
            } while (response.fileSize >= thresh && index < units.length - 1);
            message += response.fileSize.toFixed(1) + ' ' + units[index];
        }
        return message;
    }
}
