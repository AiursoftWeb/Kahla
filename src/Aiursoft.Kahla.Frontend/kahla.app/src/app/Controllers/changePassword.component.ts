import { Component } from '@angular/core';
import { AuthApiService } from '../Services/Api/AuthApiService';
import Swal from 'sweetalert2';
import { lastValueFrom } from 'rxjs';
import { showCommonErrorDialog } from '../Utils/CommonErrorDialog';

@Component({
    templateUrl: '../Views/changePassword.html',
    styleUrls: ['../Styles/menu-textbox.scss', '../Styles/button.scss'],
    standalone: false,
})
export class ChangePasswordComponent {
    public oldPassword = '';
    public newPassword = '';
    public confirmPassword = '';

    constructor(private authApiServer: AuthApiService) {}

    public async onSubmit(): Promise<void> {
        if (this.newPassword !== this.confirmPassword) {
            void Swal.fire('Passwords are not same!', 'error');
            return;
        }
        try {
            const result = await lastValueFrom(
                this.authApiServer.ChangePassword(this.oldPassword, this.newPassword)
            );
            void Swal.fire('All set', result.message, 'success');
        } catch (err) {
            showCommonErrorDialog(err);
        }
    }
}
