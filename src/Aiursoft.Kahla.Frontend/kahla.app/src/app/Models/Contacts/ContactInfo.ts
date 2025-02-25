import { KahlaUser } from '../KahlaUser';

export interface ContactInfo {
    user: KahlaUser;
    online: boolean;
    isKnownContact: boolean;
    isBlockedByYou: boolean;
}
