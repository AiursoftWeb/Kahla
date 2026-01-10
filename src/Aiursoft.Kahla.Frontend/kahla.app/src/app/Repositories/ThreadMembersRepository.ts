import { lastValueFrom } from 'rxjs';
import { ThreadMemberInfo } from '../Models/Threads/ThreadMemberInfo';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';

export class ThreadMembersRepository extends RepositoryListBase<ThreadMemberInfo> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private threadsApiService: ThreadsApiService,
        private threadId: number,
        private searchInput?: string,
        private searchExclude?: string,
        initialData?: ThreadMemberInfo[]
    ) {
        super();
        if (initialData) {
            this.data = initialData;
            this.status = 'synced';
        }
    }

    protected async requestOnline(
        take: number,
        skip: number
    ): Promise<[ThreadMemberInfo[], number]> {
        const resp = await lastValueFrom(
            this.threadsApiService.Members(
                this.threadId,
                take,
                skip,
                this.searchInput,
                this.searchExclude
            )
        );
        return [resp.members, resp.totalCount];
    }
}
