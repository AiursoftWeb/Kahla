import { AiurProtocol } from './AiurProtocol';

export interface AiurCollection<T> extends AiurProtocol {
    items: T[];
}
