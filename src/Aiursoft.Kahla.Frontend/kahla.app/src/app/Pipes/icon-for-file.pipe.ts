import { Pipe, PipeTransform } from '@angular/core';
@Pipe({
    name: 'iconForFile',
    standalone: true,
})
export class IconForFilePipe implements PipeTransform {
    transform(extension: string): string {
        switch (extension) {
            case 'pdf':
                return 'far fa-file-pdf';
            case 'jpg':
            case 'jpeg':
            case 'png':
            case 'svg':
            case 'bmp':
                return 'far fa-file-image';
            case 'mp3':
            case 'aac':
            case 'flac':
            case 'wav':
                return 'far fa-file-audio';
            case 'mp4':
            case 'avi':
            case 'mkv':
            case 'ogg':
            case 'webm':
            case 'm4v':
                return 'far fa-file-video';
            case 'zip':
            case 'tar':
            case '7z':
            case 'dmg':
            case 'tar.gz':
                return 'far fa-file-archive';
            case 'doc':
            case 'docx':
            case 'pages':
                return 'far fa-file-word';
            case 'xls':
            case 'xlsx':
            case 'numbers':
                return 'far fa-file-excel';
            case 'ppt':
            case 'pptx':
            case 'key':
                return 'far fa-file-powerpoint';
            default:
                return 'fas fa-file';
        }
    }
}
