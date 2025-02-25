import { AiurProtocol } from '../AiurProtocol';
import { ContactInfo } from './ContactInfo';

export interface ContactsListApiResponse extends AiurProtocol {
    knownContacts: ContactInfo[];
    totalKnownContacts: number;
}
