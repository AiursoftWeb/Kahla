import { lastValueFrom } from 'rxjs';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { CachedDictionaryBase } from './CachedDictionaryBase';
import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class ThreadInfoCacheDictionary extends CachedDictionaryBase<number, ThreadInfoJoined> {
    constructor(private threadsApiService: ThreadsApiService) {
        super(300, 'thread-info-joined');
    }

    protected async cacheMiss(key: number): Promise<ThreadInfoJoined> {
        return (await lastValueFrom(this.threadsApiService.DetailsJoined(key))).thread;
    }
}
