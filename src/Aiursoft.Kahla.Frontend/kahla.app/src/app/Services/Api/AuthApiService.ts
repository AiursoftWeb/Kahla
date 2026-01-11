import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/';
import { AiurProtocol } from '../../Models/AiurProtocol';
import { ApiService } from './ApiService';
import { MeApiModel } from '../../Models/ApiModels/MeApiModel';

@Injectable()
export class AuthApiService {
    private static serverPath = '/auth';

    constructor(private apiService: ApiService) {}

    public Me(): Observable<MeApiModel> {
        return this.apiService.Get(AuthApiService.serverPath + '/Me');
    }

    public UpdateMe(updateModel: {
        nickName?: string;
        bio?: string;
        iconFilePath?: string;
        themeId?: number;
        enableEmailNotification?: boolean;
        enableEnterToSendMessage?: boolean;
        enableHideMyOnlineStatus?: boolean;
        allowSearchByName?: boolean;
        allowHardInvitation?: boolean;
    }): Observable<AiurProtocol> {
        return this.apiService.Patch(AuthApiService.serverPath + '/update-me', updateModel);
    }

    public ChangePassword(oldPassword: string, newPassword: string): Observable<AiurProtocol> {
        return this.apiService.Post(AuthApiService.serverPath + '/change-password', {
            OldPassword: oldPassword,
            NewPassword: newPassword,
        });
    }

    public SendMail(email: string): Observable<AiurProtocol> {
        return this.apiService.Post(AuthApiService.serverPath + '/SendEmail', {
            email: email,
        });
    }

    public SignIn(email: string, password: string): Observable<AiurProtocol> {
        return this.apiService.Post(AuthApiService.serverPath + '/SignIn', {
            Email: email,
            Password: password,
        });
    }

    public Register(email: string, password: string): Observable<AiurProtocol> {
        return this.apiService.Post(AuthApiService.serverPath + '/Register', {
            Email: email,
            Password: password,
        });
    }

    public Signout(deviceId: number): Observable<AiurProtocol> {
        return this.apiService.Post(AuthApiService.serverPath + '/Signout', {
            DeviceId: deviceId,
        });
    }
}
