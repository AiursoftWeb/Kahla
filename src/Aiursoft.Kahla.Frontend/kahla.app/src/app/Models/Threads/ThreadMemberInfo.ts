import { ContactInfo } from '../Contacts/ContactInfo';

export interface ThreadMemberInfo extends ContactInfo {
    isAdmin: boolean;
    isOwner: boolean;
    joinTime: Date;
}
