import { AiurProtocol } from '../AiurProtocol';
import { ContactInfo } from '../Contacts/ContactInfo';
import { ThreadInfoJoined } from '../Threads/ThreadInfo';

export interface UserDetailViewModel extends AiurProtocol {
    searchedUser: ContactInfo;
    commonThreads: ThreadInfoJoined[];
    commonThreadsCount: number;
    defaultThread?: number; // id
}
