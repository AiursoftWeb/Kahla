import Swal from 'sweetalert2';

export const SwalToast = Swal.mixin({
    toast: true,
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    icon: 'success',
    title: 'Success',
    didOpen: toast => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
    },
});

export const YesNoDialog = Swal.mixin({
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Yes',
    cancelButtonText: 'No',
});

export const YesNoDialogSerious = YesNoDialog.mixin({
    input: 'checkbox',
    inputValue: 0,
    inputPlaceholder: 'Yes, I know what I am doing.',
    inputValidator: result => !result && 'You must confirm that you know what you are doing.',
});

export const WorkingDialog = Swal.mixin({
    title: 'Updating...',
    allowOutsideClick: false,
    allowEscapeKey: false,
    showConfirmButton: false,
    showCancelButton: false,
    didOpen: () => {
        Swal.showLoading();
    },
    icon: 'info',
});
