import { AiurProtocol } from '../AiurProtocol';

export interface UploadFile extends AiurProtocol {
    filePath: string;
    fileSize: number;
}
