import { AiurProtocol } from '../AiurProtocol';

export interface InitWebsocketViewModel extends AiurProtocol {
    // otp: string;
    // otpValidTo: string;
    webSocketEndpoint: string;
}
