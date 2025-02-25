import { lastValueFrom } from 'rxjs';
import { ThreadInfo } from '../Models/Threads/ThreadInfo';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { CachedDictionaryBase } from './CachedDictionaryBase';
import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class ThreadInfoPublicCacheDictionary extends CachedDictionaryBase<number, ThreadInfo> {
    constructor(private threadsApiService: ThreadsApiService) {
        super(3600, 'thread-info-public');
    }

    protected async cacheMiss(key: number): Promise<ThreadInfo> {
        return (await lastValueFrom(this.threadsApiService.DetailsAnonymous(key))).thread;
    }
}
