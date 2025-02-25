import { Component } from '@angular/core';
import { ApiService } from '../Services/Api/ApiService';
import { InitService } from '../Services/InitService';
import Swal from 'sweetalert2';
import { AuthApiService } from '../Services/Api/AuthApiService';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';

@Component({
    templateUrl: '../Views/signin.html',
    styleUrls: ['../Styles/signin.scss', '../Styles/button.scss'],
    standalone: false,
})
export class SignInComponent {
    public userName = '';
    public password = '';
    public inRegister = false;
    public confirmPassword = '';

    constructor(
        public apiService: ApiService,
        public initService: InitService,
        public authApiService: AuthApiService
    ) {}

    async login() {
        try {
            await lastValueFrom(this.authApiService.SignIn(this.userName, this.password));
            void this.initService.init();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    async register() {
        if (this.password !== this.confirmPassword) {
            void Swal.fire('Password does not match', '', 'error');
            return;
        }
        if (this.password.length < 6) {
            void Swal.fire('Password is too short', '', 'error');
            return;
        }
        if (this.userName.length < 4) {
            void Swal.fire('Username is too short', '', 'error');
            return;
        }

        try {
            await lastValueFrom(this.authApiService.Register(this.userName, this.password));
            void Swal.fire('Register success', '', 'success');
            void this.initService.init();
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }

    forgetPassword() {
        void Swal.fire('Forget Password', 'Please contact your administrator', 'info');
    }
}
