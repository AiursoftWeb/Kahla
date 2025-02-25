import { lastValueFrom } from 'rxjs';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import { SearchApiService } from '../Services/Api/SearchApiService';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';

export class ServerContactsRepository extends RepositoryListBase<ContactInfo> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private searchApiService: SearchApiService,
        private keyword: string,
        private keywordExclude?: string
    ) {
        super();
    }

    protected async requestOnline(take: number, skip: number): Promise<[ContactInfo[], number]> {
        const resp = await lastValueFrom(
            this.searchApiService.SearchUsers(this.keyword, take, skip, this.keywordExclude)
        );
        return [resp.users, resp.totalUsersCount];
    }
}
