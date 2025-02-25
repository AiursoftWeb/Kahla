import { debounceTime, Subject } from 'rxjs';

class CacheEntry<T> {
    getAsync(): Promise<T | null> {
        if (this.item != null) {
            return Promise.resolve(this.item);
        } else {
            return this.itemLoader ?? Promise.resolve(null);
        }
    }

    getSync(): T | null {
        return this.item ?? null;
    }

    constructor(
        private item?: T,
        private itemLoader?: Promise<T>,
        public readonly cachedTime = new Date()
    ) {
        if (item == null && itemLoader == null) {
            throw new Error('item and itemLoader cannot be both null');
        }

        if (item != null && itemLoader != null) {
            throw new Error('item and itemLoader cannot be both set');
        }

        if (itemLoader != null) {
            itemLoader
                .then(value => {
                    this.item = value;
                    this.itemLoader = undefined;
                })
                .catch(err => {
                    console.error(err);
                    this.item = undefined;
                    this.itemLoader = undefined;
                });
        }
    }
}

interface PersistCacheEntry<T> {
    value: T;
    cachedTime: number;
}

export abstract class CachedDictionaryBase<TKey, TValue> {
    protected cache = new Map<TKey, CacheEntry<TValue>>();

    protected savePersist$ = new Subject<void>();

    constructor(
        public readonly ttlSeconds: number,
        public readonly persistKey?: string
    ) {
        if (this.persistKey != null) {
            const persistCache = localStorage.getItem('cache-dict-' + this.persistKey);
            if (persistCache) {
                const cacheEntries = new Map<TKey, PersistCacheEntry<TValue>>(
                    JSON.parse(persistCache) as [TKey, PersistCacheEntry<TValue>][]
                );
                cacheEntries.forEach((value, key) => {
                    this.cache.set(
                        key,
                        new CacheEntry<TValue>(value.value, undefined, new Date(value.cachedTime))
                    );
                });
            }

            this.savePersist$.pipe(debounceTime(1000)).subscribe(() => {
                this.saveToStorage();
            });
        }
    }

    public async get(key: TKey, forceRenew = false): Promise<TValue> {
        if (
            this.cache.has(key) &&
            !forceRenew &&
            new Date().getTime() - this.cache.get(key)!.cachedTime.getTime() <
                this.ttlSeconds * 1000
        ) {
            const res = await this.cache.get(key)!.getAsync();
            if (res != null) return res;
        }
        const value = this.cacheMiss(key);
        this.cache.set(key, new CacheEntry<TValue>(undefined, value));
        const valComputed = await value;
        this.savePersist$.next();
        return valComputed;
    }

    public set(key: TKey, value: TValue) {
        this.cache.set(key, new CacheEntry<TValue>(value));
        this.savePersist$.next();
    }

    public delete(key: TKey) {
        if (!this.cache.has(key)) return;
        this.cache.delete(key);
        this.savePersist$.next();
    }

    public saveToStorage() {
        if (this.persistKey == null) return;
        const persistCache = new Map<TKey, PersistCacheEntry<TValue>>();
        this.cache.forEach((value, key) => {
            if (
                value.getSync() != null &&
                new Date().getTime() - value.cachedTime.getTime() < this.ttlSeconds * 1000
            )
                persistCache.set(key, {
                    value: value.getSync()!,
                    cachedTime: value.cachedTime.getTime(),
                });
        });

        localStorage.setItem(
            'cache-dict-' + this.persistKey,
            JSON.stringify(Array.from(persistCache.entries()))
        );
    }

    protected abstract cacheMiss(key: TKey): Promise<TValue>;
}
