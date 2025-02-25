import { lastValueFrom } from 'rxjs';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';
import { ContactsApiService } from '../Services/Api/ContactsApiService';

export class CommonThreadRepository extends RepositoryListBase<ThreadInfoJoined> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private contactsApiService: ContactsApiService,
        private userId: string
    ) {
        super();
    }

    protected async requestOnline(
        take: number,
        skip: number
    ): Promise<[ThreadInfoJoined[], number]> {
        const resp = await lastValueFrom(this.contactsApiService.Details(this.userId, take, skip));
        return [resp.commonThreads, resp.commonThreadsCount];
    }
}
