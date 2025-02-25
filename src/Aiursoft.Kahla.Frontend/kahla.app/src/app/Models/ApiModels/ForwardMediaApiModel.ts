import { AiurProtocol } from '../AiurProtocol';

export interface ForwardMediaApiModel extends AiurProtocol {
    siteName: string;
    filePath: string;
    internetPath: string;
    fileSize: string;
}
