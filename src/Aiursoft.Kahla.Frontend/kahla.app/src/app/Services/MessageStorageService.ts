import { Injectable } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import type { KahlaCommit, ChatMessage, KahlaMessagesRepo } from '@aiursoft/kahla.sdk';
import {
    KahlaMessagesIndexedDBStore,
    LoadMessagesResult,
    LoadDirection,
} from '../Storage/KahlaMessagesIndexedDBStore';
import { ThreadSyncState } from '../Storage/MessageDatabase';

/**
 * Event emitted when messages are loaded from storage.
 */
export interface MessagesLoadedEvent {
    threadId: number;
    messages: KahlaCommit<ChatMessage>[];
    hasMore: boolean;
    source: 'initial' | 'pagination' | 'sync';
}

/**
 * Angular service for managing message persistence with IndexedDB.
 * Provides a bridge between the KahlaMessagesRepo (in-memory sync) and
 * persistent IndexedDB storage.
 * 
 * Key features:
 * - Automatic persistence of incoming/outgoing messages
 * - Paginated loading for long conversations
 * - Sync state restoration for resuming connections
 * - Fragment tracking for handling message gaps
 */
@Injectable({
    providedIn: 'root',
})
export class MessageStorageService {
    private store = new KahlaMessagesIndexedDBStore();
    private initialized = false;
    private initPromise: Promise<void> | null = null;

    /** Emitted when messages are loaded from storage */
    public messagesLoaded$ = new Subject<MessagesLoadedEvent>();

    /** Current loading state per thread */
    private loadingState = new Map<number, BehaviorSubject<boolean>>();

    /** Number of messages to load initially */
    private readonly INITIAL_LOAD_COUNT = 50;

    /** Number of messages to load per pagination request */
    private readonly PAGE_SIZE = 30;

    constructor() {
        this.initPromise = this.init();
    }

    /**
     * Initialize the storage service.
     */
    private async init(): Promise<void> {
        if (this.initialized) return;
        await this.store.init();
        this.initialized = true;
    }

    /**
     * Ensure the service is initialized before use.
     */
    private async ensureInitialized(): Promise<void> {
        if (!this.initialized && this.initPromise) {
            await this.initPromise;
        }
    }

    /**
     * Get loading state observable for a thread.
     */
    public getLoadingState(threadId: number): BehaviorSubject<boolean> {
        if (!this.loadingState.has(threadId)) {
            this.loadingState.set(threadId, new BehaviorSubject<boolean>(false));
        }
        return this.loadingState.get(threadId)!;
    }

    // ========== Initialization & Restoration ==========

    /**
     * Get stored sync state for a thread.
     * Used to restore sync position when reconnecting.
     */
    async getSyncState(threadId: number): Promise<ThreadSyncState | null> {
        await this.ensureInitialized();
        return this.store.getSyncState(threadId);
    }

    /**
     * Load initial messages from storage for a thread.
     * Call this before connecting to the WebSocket to show cached messages.
     * 
     * @returns Initial messages and sync state
     */
    async loadInitialMessages(threadId: number): Promise<{
        messages: KahlaCommit<ChatMessage>[];
        syncState: ThreadSyncState | null;
        hasMore: boolean;
    }> {
        await this.ensureInitialized();

        const loading = this.getLoadingState(threadId);
        loading.next(true);

        try {
            const [result, syncState] = await Promise.all([
                this.store.loadLatest(threadId, this.INITIAL_LOAD_COUNT),
                this.store.getSyncState(threadId),
            ]);

            this.messagesLoaded$.next({
                threadId,
                messages: result.messages,
                hasMore: result.hasMore,
                source: 'initial',
            });

            return {
                messages: result.messages,
                syncState,
                hasMore: result.hasMore,
            };
        } finally {
            loading.next(false);
        }
    }

    /**
     * Load older messages for pagination.
     * 
     * @param threadId - Thread ID
     * @param beforeSeq - Load messages before this sequence index
     */
    async loadOlderMessages(
        threadId: number,
        beforeSeq: number
    ): Promise<LoadMessagesResult> {
        await this.ensureInitialized();

        const loading = this.getLoadingState(threadId);
        loading.next(true);

        try {
            const result = await this.store.loadRange(
                threadId,
                beforeSeq - 1,
                this.PAGE_SIZE,
                'backward'
            );

            this.messagesLoaded$.next({
                threadId,
                messages: result.messages,
                hasMore: result.hasMore,
                source: 'pagination',
            });

            return result;
        } finally {
            loading.next(false);
        }
    }

    // ========== Message Persistence ==========

    /**
     * Persist a single message to storage.
     * Called when a message is received via WebSocket or sent locally.
     */
    async persistMessage(
        threadId: number,
        commit: KahlaCommit<ChatMessage>,
        seqIndex: number
    ): Promise<void> {
        await this.ensureInitialized();
        await this.store.saveBatch(threadId, [commit], seqIndex);
    }

    /**
     * Persist multiple messages in batch.
     * More efficient for initial sync or bulk operations.
     */
    async persistMessages(
        threadId: number,
        commits: KahlaCommit<ChatMessage>[],
        startSeqIndex: number
    ): Promise<void> {
        await this.ensureInitialized();
        await this.store.saveBatch(threadId, commits, startSeqIndex);
    }

    /**
     * Update sync state after successful sync operations.
     */
    async updateSyncState(
        threadId: number,
        pulledItemsOffset: number,
        pushedItemsOffset: number,
        lastPulledCommitId: string | null,
        lastPushedCommitId: string | null
    ): Promise<void> {
        await this.ensureInitialized();
        await this.store.updateSyncOffsets(
            threadId,
            pulledItemsOffset,
            pushedItemsOffset,
            lastPulledCommitId,
            lastPushedCommitId
        );
    }

    // ========== Repo Integration ==========

    /**
     * Subscribe to a KahlaMessagesRepo to automatically persist messages.
     * Returns a cleanup function to unsubscribe.
     * 
     * @param threadId - Thread ID
     * @param repo - KahlaMessagesRepo instance
     */
    subscribeToRepo(
        threadId: number,
        repo: KahlaMessagesRepo
    ): () => void {
        let seqIndex = 0;

        // Load initial seq index
        this.store.getMaxSeqIndex(threadId).then(maxSeq => {
            seqIndex = maxSeq + 1;
        });

        // Subscribe to message changes
        const messageSub = repo.messages.messages.onChange.subscribe(async event => {
            const commit = event.newNode.value;
            
            // Persist the new message
            await this.persistMessage(threadId, commit, seqIndex);
            seqIndex++;
        });

        // Subscribe to pointer changes for sync state updates
        const pointerSub = repo.messages.onPointersChanged.subscribe(async () => {
            const messages = repo.messages;
            await this.updateSyncState(
                threadId,
                messages.pulledItemsOffset,
                messages.pushedItemsOffset,
                messages.lastPulled?.value.id ?? null,
                messages.lastPushed?.value.id ?? null
            );
        });

        // Return cleanup function
        return () => {
            messageSub.unsubscribe();
            pointerSub.unsubscribe();
        };
    }

    /**
     * Restore messages from storage into a KahlaMessagesRepo.
     * Call this before connecting to pre-populate with cached messages.
     * 
     * Note: This loads messages into memory, so use sparingly for very long threads.
     * For very long threads, consider only restoring the sync state and
     * letting the UI handle pagination.
     * 
     * @param threadId - Thread ID
     * @param repo - KahlaMessagesRepo instance
     * @param maxMessages - Maximum messages to restore (default: 100)
     */
    async restoreToRepo(
        threadId: number,
        repo: KahlaMessagesRepo,
        maxMessages = 100
    ): Promise<void> {
        await this.ensureInitialized();

        const [result, syncState] = await Promise.all([
            this.store.loadLatest(threadId, maxMessages),
            this.store.getSyncState(threadId),
        ]);

        // Restore sync state offsets
        if (syncState) {
            repo.messages.pulledItemsOffset = syncState.pulledItemsOffset;
            repo.messages.pushedItemsOffset = syncState.pushedItemsOffset;
        }

        // Add messages to the repo's in-memory store
        for (const commit of result.messages) {
            await repo.messages.commitCommit(commit);
            // Advance pointers as if these were already synced
            if (syncState) {
                const node = repo.messages.messages.last;
                if (node && syncState.lastPulledCommitId === commit.id) {
                    repo.messages.lastPulled = node;
                }
                if (node && syncState.lastPushedCommitId === commit.id) {
                    repo.messages.lastPushed = node;
                }
            }
        }
    }

    // ========== Fragment Management ==========

    /**
     * Check if there are gaps in the stored messages for a thread.
     * 
     * @param threadId - Thread ID
     * @param fromSeq - Start of range to check
     * @param toSeq - End of range to check
     */
    async hasGaps(threadId: number, fromSeq: number, toSeq: number): Promise<boolean> {
        await this.ensureInitialized();
        const gaps = await this.store.findGaps(threadId, fromSeq, toSeq);
        return gaps.length > 0;
    }

    /**
     * Record a fragment of continuous messages.
     * Used after successfully syncing a range of messages.
     */
    async recordFragment(threadId: number, startSeq: number, endSeq: number): Promise<void> {
        await this.ensureInitialized();
        await this.store.addFragment(threadId, startSeq, endSeq);
    }

    // ========== Utility Methods ==========

    /**
     * Get the total message count for a thread.
     */
    async getMessageCount(threadId: number): Promise<number> {
        await this.ensureInitialized();
        return this.store.getMessageCount(threadId);
    }

    /**
     * Clear all stored messages for a thread.
     */
    async clearThread(threadId: number): Promise<void> {
        await this.ensureInitialized();
        await this.store.clearThread(threadId);
        this.loadingState.delete(threadId);
    }

    /**
     * Get storage statistics for debugging.
     */
    async getStats(): Promise<{
        totalMessages: number;
        threadCount: number;
        threads: Array<{ threadId: number; messageCount: number }>;
    }> {
        await this.ensureInitialized();
        return this.store.getStats();
    }

    /**
     * Check if a message exists in storage.
     */
    async hasMessage(threadId: number, commitId: string): Promise<boolean> {
        await this.ensureInitialized();
        const msg = await this.store.findByCommitId(threadId, commitId);
        return msg !== undefined;
    }
}
