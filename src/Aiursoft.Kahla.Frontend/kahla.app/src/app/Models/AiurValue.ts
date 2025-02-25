import { AiurProtocol } from './AiurProtocol';

export interface AiurValue<T> extends AiurProtocol {
    value: T;
}

export type AiurValueNamed<T, N extends string> = Record<N, T> & AiurProtocol;
