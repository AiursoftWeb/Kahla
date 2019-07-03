import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthApiService } from '../Services/AuthApiService';
import { UploadService } from '../Services/UploadService';
import { KahlaUser } from '../Models/KahlaUser';
import { AiurProtocal } from '../Models/AiurProtocal';
import { AiurCollection } from '../Models/AiurCollection';
import Swal from 'sweetalert2';
import { Values } from '../values';
import { MessageService } from '../Services/MessageService';
import { ElectronService } from 'ngx-electron';

@Component({
  templateUrl: '../Views/userDetail.html',
  styleUrls: [
    '../Styles/userDetail.scss',
    '../Styles/button.scss',
      '../Styles/toggleButton.scss'
  ]
})

export class UserDetailComponent implements OnInit {
  public user: KahlaUser;
  public loadingImgURL = Values.loadingImgURL;
    @ViewChild('imageInput', {static: false}) public imageInput;
  constructor(
    private authApiService: AuthApiService,
    private router: Router,
    public uploadService: UploadService,
    public messageService: MessageService,
    public _electronService: ElectronService
  ) {
}

  public ngOnInit(): void {
    if (!this.messageService.me) {
      this.authApiService.Me().subscribe(p => {
        this.user = p.value;
        this.user.avatarURL = Values.fileAddress + this.user.headImgFileKey;
      });
    } else {
      this.user = Object.assign({}, this.messageService.me);
    }
  }

  public uploadAvatar(): void {
    if (this.imageInput) {
      const fileBrowser = this.imageInput.nativeElement;
      if (fileBrowser.files && fileBrowser.files[0]) {
        this.uploadService.uploadAvatar(this.user, fileBrowser.files[0]);
      }
    }
  }

  public save(): void {
    const saveButton =  document.querySelector('#save');
    saveButton.textContent = 'Saving...';
    const hideEmail = (<HTMLInputElement>document.querySelector('#toggleHideEmail')).checked;
    this.user.makeEmailPublic = !hideEmail;
    this.authApiService.UpdateInfo(this.user.nickName, this.user.bio, this.user.headImgFileKey, hideEmail)
      .subscribe((response) => {
        if (response.code === 0) {
          this.messageService.me = Object.assign({}, this.user);
            this.router.navigate(['/home']);
        } else {
          Swal.fire('Error', (response as AiurProtocal as AiurCollection<string>).items[0], 'error');
        }
        saveButton.textContent = 'Save';
    });
  }
}
