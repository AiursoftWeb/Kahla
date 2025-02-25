import { lastValueFrom } from 'rxjs';
import { ContactInfo } from '../Models/Contacts/ContactInfo';
import { BlocksApiService } from '../Services/Api/BlocksApiService';
import { RepositoryListBase, RepositoryPersistConfig } from './RepositoryBase';

export class MyBlocksRepository extends RepositoryListBase<ContactInfo> {
    protected readonly persistConfig: RepositoryPersistConfig = {
        persist: false,
    };

    constructor(
        private blocksApiService: BlocksApiService,
        private keyword?: string,
        private excluding?: string
    ) {
        super();
    }

    protected async requestOnline(take: number, skip: number): Promise<[ContactInfo[], number]> {
        const resp = await lastValueFrom(
            this.blocksApiService.List(take, skip, this.keyword, this.excluding)
        );
        return [resp.knownBlocks, resp.totalKnownBlocks];
    }
}
