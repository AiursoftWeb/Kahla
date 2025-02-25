import { AiurProtocol } from '../AiurProtocol';
import { AppOptions } from '../AppOptions';
import { KahlaUser } from '../KahlaUser';

export interface MeApiModel extends AiurProtocol {
    user: KahlaUser;

    privateSettings: AppOptions;
}
