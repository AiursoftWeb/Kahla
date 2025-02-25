import { logger } from '../Services/Logger';

interface RepositoryCache<T> {
    data: T[];
    version: number;
}

export type RepositoryStatus = 'loading' | 'synced' | 'error' | 'offline' | 'uninitialized';

export interface RepositoryPersistConfig {
    name?: string;
    version?: number;
    persist: boolean;
}

export interface RepositoryLike<T> {
    data: T[];
    status: RepositoryStatus;
    saveCache(): void;
    initCache(): void;
    updateAll(): Promise<void>;
    loadMore(take: number): Promise<void>;
    canLoadMore: boolean;
    health: boolean;
}

export abstract class RepositoryBase<T> implements RepositoryLike<T> {
    protected abstract get persistConfig(): RepositoryPersistConfig;
    public data: T[] = [];
    public status: RepositoryStatus = 'uninitialized';

    public saveCache(): void {
        localStorage.setItem(
            `repo-cache-${this.persistConfig.name!}`,
            JSON.stringify({
                data: this.data,
                version: this.persistConfig.version!,
            } satisfies RepositoryCache<T>)
        );
    }

    public initCache() {
        if (localStorage.getItem(`repo-cache-${this.persistConfig.name!}`)) {
            const data = JSON.parse(
                localStorage.getItem(`repo-cache-${this.persistConfig.name!}`)!
            ) as RepositoryCache<T>;
            if (data.version !== this.persistConfig.version!) {
                this.data = [];
                this.saveCache();
            }
            this.data = data.data;
            this.status = 'offline';
        } else {
            this.data = [];
        }
    }

    public get health(): boolean {
        return this.status === 'synced' || this.status === 'loading';
    }

    protected updatePromise?: Promise<void>;
    protected loadMorePromise?: Promise<void>;

    public updateAll(): Promise<void> {
        return (
            this.updatePromise ??
            (this.updatePromise = (async () => {
                this.status = 'loading';
                try {
                    await this.updateAllInternal();
                    this.status = 'synced';
                    this.saveCache();
                } catch (err) {
                    this.status = 'error';
                    logger.error('Repository update error', err);
                    throw err;
                } finally {
                    this.updatePromise = undefined;
                }
            })())
        );
    }

    public loadMore(take: number): Promise<void> {
        if (!this.canLoadMore) throw new Error('Cannot load more');
        return (
            this.loadMorePromise ??
            (this.loadMorePromise = (async () => {
                try {
                    this.status = 'loading';
                    await this.loadMoreInternal(take);
                    this.saveCache();
                } finally {
                    // When on error, new items will be rejected, but the previous items will still be kept
                    this.status = 'synced';
                }
            })())
        );
    }

    protected abstract updateAllInternal(): Promise<void>;

    protected abstract loadMoreInternal(take: number): Promise<void>;

    public abstract canLoadMore: boolean;
}

export abstract class RepositoryListBase<T> extends RepositoryBase<T> {
    public total: number;

    protected async updateAllInternal(): Promise<void> {
        const [resp, total] = await this.requestOnline(20, 0);
        this.data = resp;
        this.total = total;
    }

    protected async loadMoreInternal(take: number): Promise<void> {
        const [resp, total] = await this.requestOnline(take, this.data.length);
        this.data = this.data.concat(resp);
        this.total = total;
    }

    public get canLoadMore(): boolean {
        // Total = -1: The repo is just restore from localstorage, we don't know the total
        return this.total === -1 || this.data.length < this.total;
    }

    protected abstract requestOnline(take: number, skip: number): Promise<[T[], number]>;
}

export class MappedRepository<TOut, TIn> implements RepositoryLike<TOut> {
    constructor(
        private srcRepo: RepositoryLike<TIn>,
        private mapFunc: (src: TIn) => TOut
    ) {}
    public get data(): TOut[] {
        return this.srcRepo.data.map(this.mapFunc);
    }
    public get status(): RepositoryStatus {
        return this.srcRepo.status;
    }
    saveCache(): void {
        this.srcRepo.saveCache();
    }
    initCache(): void {
        this.srcRepo.initCache();
    }
    updateAll(): Promise<void> {
        return this.srcRepo.updateAll();
    }
    loadMore(take: number): Promise<void> {
        return this.srcRepo.loadMore(take);
    }
    get canLoadMore(): boolean {
        return this.srcRepo.canLoadMore;
    }
    get health(): boolean {
        return this.srcRepo.health;
    }
}

export class StaticRepository<T> implements RepositoryLike<T> {
    constructor(public data: T[]) {}
    public readonly status = 'synced';
    saveCache(): void {
        // Do nothing
    }
    initCache(): void {
        // Do nothing
    }
    updateAll(): Promise<void> {
        return Promise.resolve();
    }
    loadMore(): Promise<void> {
        return Promise.resolve();
    }
    readonly canLoadMore = false;
    readonly health = true;
}
