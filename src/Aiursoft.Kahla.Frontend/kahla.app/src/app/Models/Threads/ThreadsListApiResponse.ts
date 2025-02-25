import { AiurProtocol } from '../AiurProtocol';
import { ThreadInfoJoined } from './ThreadInfo';

export interface ThreadsListApiResponse extends AiurProtocol {
    knownThreads: ThreadInfoJoined[];
    totalCount: number;
}
