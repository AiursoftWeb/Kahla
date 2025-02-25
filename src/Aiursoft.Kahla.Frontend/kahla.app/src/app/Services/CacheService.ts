import { Injectable, Signal } from '@angular/core';
import { ServerConfig } from '../Models/ServerConfig';
import { CachedObject } from '../Caching/CachedObject';
import { AuthApiService } from './Api/AuthApiService';
import { lastValueFrom } from 'rxjs';
import { MeCacheModel } from '../Models/CacheModel';
import { toSignal } from '@angular/core/rxjs-interop';

@Injectable({
    providedIn: 'root',
})
export class CacheService {
    public mine: Signal<MeCacheModel | null>;
    public mineCache: CachedObject<MeCacheModel>;
    public totalUnread = 0;
    public serverConfig: ServerConfig;

    constructor(authApiService: AuthApiService) {
        this.mineCache = new CachedObject('mine', async () => {
            const resp = await lastValueFrom(authApiService.Me());
            return {
                me: resp.user,
                privateSettings: resp.privateSettings,
            };
        });
        this.mine = toSignal(this.mineCache.itemUpdated$, { requireSync: true });
    }
}
