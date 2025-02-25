import { AiurProtocol } from '../AiurProtocol';
import { ContactInfo } from '../Contacts/ContactInfo';

export interface BlocksListApiResponse extends AiurProtocol {
    knownBlocks: ContactInfo[];
    totalKnownBlocks: number;
}
