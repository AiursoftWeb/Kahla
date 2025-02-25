import { AiurValue } from '../AiurValue';

export interface FileTokenApiModel extends AiurValue<string> {
    uploadAddress: string;
}
