import { BehaviorSubject, debounceTime, skip } from 'rxjs';

export class CachedObject<T> {
    private item?: T;
    private updatePromise?: Promise<void>;

    public itemUpdated$ = new BehaviorSubject<T | null>(null);

    constructor(
        private cacheKey: string,
        private updater?: () => Promise<T>,
        parseTransform: (data: T) => T = data => data
    ) {
        if (localStorage.getItem('cache-obj-' + cacheKey)) {
            this.itemUpdated$.next(
                parseTransform(JSON.parse(localStorage.getItem('cache-obj-' + cacheKey)!) as T)
            );
        }

        this.itemUpdated$.subscribe(t => (this.item = t ?? undefined));
        this.itemUpdated$.pipe(skip(1), debounceTime(100)).subscribe(t => this.saveToStorage(t!));
    }

    public update(): Promise<void> {
        if (this.updater == null) {
            throw new Error('No updater provided, cannot update.');
        }
        return this.updatePromise ?? (this.updatePromise = this.updateInternal());
    }

    private async updateInternal() {
        if (this.updater == null) return;
        try {
            this.itemUpdated$.next(await this.updater());
        } finally {
            this.updatePromise = undefined;
        }
    }

    public async get(): Promise<T> {
        if (this.updatePromise) {
            await this.updatePromise;
        }
        if (this.item != null) {
            return this.item;
        }

        if (this.updater == null) {
            throw new Error('No updater provided');
        }

        await this.update();
        return this.item!;
    }

    public getSync(): T | null {
        return this.item ?? null;
    }

    public set(data: T) {
        this.itemUpdated$.next(data);
    }

    private saveToStorage(item: T) {
        localStorage.setItem('cache-obj-' + this.cacheKey, JSON.stringify(item));
    }
}
