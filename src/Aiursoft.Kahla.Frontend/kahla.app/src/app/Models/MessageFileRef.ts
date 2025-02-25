import { FileType } from './FileType';

export class MessageFileRef {
    public fileType: FileType;
    public filePath: string;

    public imgWidth: number;
    public imgHeight: number;
    public imgDisplayWidth: number;
    public imgDisplayHeight: number;

    public fileName: string;
    public fileSize: string;
}
