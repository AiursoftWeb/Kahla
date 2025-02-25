import { AiurProtocol } from '../AiurProtocol';
import { KahlaUser } from '../KahlaUser';

export interface UserInfoViewModel extends AiurProtocol {
    briefUser: KahlaUser;
}
