import { Injectable } from '@angular/core';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';
import { lastValueFrom } from 'rxjs';

export class MyThreadsRepositoryFiltered extends RepositoryListBase<ThreadInfoJoined> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private threadsApiService: ThreadsApiService,
        private keyword?: string,
        private keywordExclude?: string
    ) {
        super();
    }

    protected async requestOnline(
        take: number,
        skip: number
    ): Promise<[ThreadInfoJoined[], number]> {
        const resp = await lastValueFrom(
            this.threadsApiService.Search(take, skip, this.keyword, this.keywordExclude)
        );
        return [resp.knownThreads, resp.totalCount];
    }
}

@Injectable({
    providedIn: 'root',
})
export class MyThreadsRepository extends MyThreadsRepositoryFiltered {
    constructor(threadsApiService: ThreadsApiService) {
        super(threadsApiService);
    }

    protected readonly persistConfig: RepositoryPersistConfig = {
        name: 'threads',
        version: 1,
        persist: true,
    };
}
