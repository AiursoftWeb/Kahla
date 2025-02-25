import { AiurProtocol } from '../AiurProtocol';
import { ContactInfo } from '../Contacts/ContactInfo';

export interface UserSearchResult extends AiurProtocol {
    totalUsersCount: number;
    users: ContactInfo[];
}
