import { Injectable } from '@angular/core';
import { FilesApiService } from './Api/FilesApiService';
import Swal, { SweetAlertResult } from 'sweetalert2';
import { KahlaUser } from '../Models/KahlaUser';
import { UploadFile } from '../Models/Probe/UploadFile';

@Injectable({
    providedIn: 'root',
})
export class UploadService {
    constructor(private filesApiService: FilesApiService) {}

    public uploadAvatar(user: KahlaUser, file: File): void {
        if (this.validImageType(file, true)) {
            const formData = new FormData();
            formData.append('file', file);
            const alert = this.fireUploadingAlert('Uploading your avatar...');
            this.filesApiService.InitIconUpload().subscribe(response => {
                if (response.code === 0) {
                    const mission = this.filesApiService
                        .UploadFile(formData, response.value)
                        .subscribe(res => {
                            if (Number(res)) {
                                this.updateAlertProgress(Number(res));
                            } else if (res != null && (res as UploadFile).code === 0) {
                                void Swal.close();
                                user.iconFilePath = (res as UploadFile).filePath;
                            }
                        });
                    void alert.then(result => {
                        if (result.dismiss) {
                            mission.unsubscribe();
                        }
                    });
                }
            });
        } else {
            void Swal.fire('Try again', 'Only support .png, .jpg, .jpeg or .bmp file', 'error');
        }
    }

    private fireUploadingAlert(title: string): Promise<SweetAlertResult> {
        const result = Swal.fire({
            title: title,
            html: '<div id="progressText">0%</div><progress id="uploadProgress" max="100" style="width: 100%"></progress>',
            showCancelButton: true,
            showConfirmButton: false,
            allowOutsideClick: false,
        });
        Swal.showLoading();
        Swal.enableButtons();
        return result;
    }

    private updateAlertProgress(progress: number): void {
        const htmlContainer = Swal.getHtmlContainer();
        if (htmlContainer) {
            const progressElement = htmlContainer.querySelector(
                '#uploadProgress'
            ) as HTMLProgressElement;
            if (progressElement) {
                progressElement.value = progress;
            }
            const progressText = htmlContainer.querySelector('#progressText') as HTMLDivElement;
            if (progressText) {
                progressText.innerText = `${progress}%`;
            }
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
}