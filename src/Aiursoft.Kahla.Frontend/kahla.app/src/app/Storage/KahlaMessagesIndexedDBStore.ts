import type { KahlaCommit, ChatMessage } from '@aiursoft/kahla.sdk';
import {
    getMessageDatabase,
    StoredMessage,
    ThreadSyncState,
    MessageFragment,
    KahlaMessageDBSchema,
} from './MessageDatabase';
import type { IDBPDatabase } from 'idb';

/**
 * Direction for loading messages from storage.
 */
export type LoadDirection = 'forward' | 'backward';

/**
 * Result of loading messages with pagination info.
 */
export interface LoadMessagesResult {
    messages: KahlaCommit<ChatMessage>[];
    hasMore: boolean;
    loadedFrom: number;
    loadedTo: number;
}

/**
 * IndexedDB-backed message store for Kahla.
 * Provides high-performance persistence with support for:
 * - Paginated loading (doesn't load all messages into memory)
 * - Fragment tracking for handling message gaps
 * - Batch operations for efficient writes
 */
export class KahlaMessagesIndexedDBStore {
    private db: IDBPDatabase<KahlaMessageDBSchema> | null = null;

    /**
     * Initialize the database connection.
     * Must be called before using other methods.
     */
    async init(): Promise<void> {
        if (!this.db) {
            this.db = await getMessageDatabase();
        }
    }

    /**
     * Ensure database is initialized.
     */
    private async ensureDb(): Promise<IDBPDatabase<KahlaMessageDBSchema>> {
        if (!this.db) {
            await this.init();
        }
        return this.db!;
    }

    // ========== Message CRUD Operations ==========

    /**
     * Save a single message to storage.
     * Automatically assigns sequence index based on sync state.
     */
    async saveMessage(threadId: number, commit: KahlaCommit<ChatMessage>): Promise<void> {
        const db = await this.ensureDb();

        // Check if message already exists
        const existing = await this.findByCommitId(threadId, commit.id);
        if (existing) {
            return; // Skip duplicate
        }

        // Get current sync state to determine sequence index
        const syncState = await this.getSyncState(threadId);
        const seqIndex = syncState ? syncState.pulledItemsOffset : 0;

        const storedMessage: StoredMessage = {
            threadId,
            seqIndex,
            commitId: commit.id,
            content: commit.item.content,
            preview: commit.item.preview,
            senderId: commit.item.senderId,
            ats: commit.item.ats,
            commitTime: commit.commitTime.getTime(),
        };

        await db.put('messages', storedMessage);
    }

    /**
     * Save multiple messages in a single transaction.
     * More efficient than calling saveMessage repeatedly.
     * 
     * @param threadId - Thread ID
     * @param commits - Array of commits to save
     * @param startSeqIndex - Starting sequence index for the batch
     */
    async saveBatch(
        threadId: number,
        commits: KahlaCommit<ChatMessage>[],
        startSeqIndex: number
    ): Promise<void> {
        if (commits.length === 0) return;

        const db = await this.ensureDb();
        const tx = db.transaction('messages', 'readwrite');
        const store = tx.objectStore('messages');

        let seqIndex = startSeqIndex;
        for (const commit of commits) {
            // Check for duplicates using the index
            const existingKey = await store.index('by-commit-id').getKey([threadId, commit.id]);
            if (existingKey) {
                continue; // Skip duplicate
            }

            const storedMessage: StoredMessage = {
                threadId,
                seqIndex,
                commitId: commit.id,
                content: commit.item.content,
                preview: commit.item.preview,
                senderId: commit.item.senderId,
                ats: commit.item.ats,
                commitTime: commit.commitTime.getTime(),
            };

            await store.put(storedMessage);
            seqIndex++;
        }

        await tx.done;
    }

    /**
     * Load a range of messages from storage.
     * Supports both forward and backward pagination.
     * 
     * @param threadId - Thread ID
     * @param fromSeq - Starting sequence index
     * @param count - Maximum number of messages to load
     * @param direction - 'forward' loads newer, 'backward' loads older
     */
    async loadRange(
        threadId: number,
        fromSeq: number,
        count: number,
        direction: LoadDirection = 'backward'
    ): Promise<LoadMessagesResult> {
        const db = await this.ensureDb();
        const tx = db.transaction('messages', 'readonly');
        const index = tx.objectStore('messages').index('by-seq');

        const messages: KahlaCommit<ChatMessage>[] = [];
        let loadedFrom = fromSeq;
        let loadedTo = fromSeq;

        if (direction === 'backward') {
            // Load older messages (descending order)
            const range = IDBKeyRange.bound([threadId, 0], [threadId, fromSeq]);
            let cursor = await index.openCursor(range, 'prev');
            let loaded = 0;

            while (cursor && loaded < count) {
                const stored = cursor.value;
                messages.unshift(this.storedToCommit(stored));
                loadedFrom = Math.min(loadedFrom, stored.seqIndex);
                loadedTo = Math.max(loadedTo, stored.seqIndex);
                loaded++;
                cursor = await cursor.continue();
            }

            // Check if there are more older messages
            const hasMore = cursor !== null;
            return { messages, hasMore, loadedFrom, loadedTo };
        } else {
            // Load newer messages (ascending order)
            const range = IDBKeyRange.lowerBound([threadId, fromSeq]);
            let cursor = await index.openCursor(range, 'next');
            let loaded = 0;

            while (cursor && loaded < count) {
                const stored = cursor.value;
                if (stored.threadId !== threadId) break;
                
                messages.push(this.storedToCommit(stored));
                loadedFrom = Math.min(loadedFrom, stored.seqIndex);
                loadedTo = Math.max(loadedTo, stored.seqIndex);
                loaded++;
                cursor = await cursor.continue();
            }

            // Check if there are more newer messages
            const hasMore = cursor !== null && cursor.value.threadId === threadId;
            return { messages, hasMore, loadedFrom, loadedTo };
        }
    }

    /**
     * Load the most recent messages for a thread.
     * Useful for initial display when opening a conversation.
     * 
     * @param threadId - Thread ID
     * @param count - Maximum number of messages to load
     */
    async loadLatest(threadId: number, count: number): Promise<LoadMessagesResult> {
        const db = await this.ensureDb();
        const tx = db.transaction('messages', 'readonly');
        const index = tx.objectStore('messages').index('by-thread');

        // Get the highest sequence index for this thread
        const allForThread = await index.getAll(threadId);
        if (allForThread.length === 0) {
            return { messages: [], hasMore: false, loadedFrom: 0, loadedTo: 0 };
        }

        // Sort by seqIndex descending and take the last N
        allForThread.sort((a, b) => b.seqIndex - a.seqIndex);
        const latest = allForThread.slice(0, count);
        
        // Reverse to get ascending order for display
        latest.reverse();

        const messages = latest.map(stored => this.storedToCommit(stored));
        const loadedFrom = latest.length > 0 ? latest[0].seqIndex : 0;
        const loadedTo = latest.length > 0 ? latest[latest.length - 1].seqIndex : 0;
        const hasMore = allForThread.length > count;

        return { messages, hasMore, loadedFrom, loadedTo };
    }

    /**
     * Find a message by its commit ID.
     * Used for deduplication.
     */
    async findByCommitId(threadId: number, commitId: string): Promise<StoredMessage | undefined> {
        const db = await this.ensureDb();
        return db.getFromIndex('messages', 'by-commit-id', [threadId, commitId]);
    }

    /**
     * Get total message count for a thread.
     */
    async getMessageCount(threadId: number): Promise<number> {
        const db = await this.ensureDb();
        return db.countFromIndex('messages', 'by-thread', threadId);
    }

    /**
     * Get the maximum sequence index for a thread.
     * Returns -1 if no messages exist.
     */
    async getMaxSeqIndex(threadId: number): Promise<number> {
        const db = await this.ensureDb();
        const tx = db.transaction('messages', 'readonly');
        const index = tx.objectStore('messages').index('by-seq');
        
        // Get the last message for this thread
        const range = IDBKeyRange.bound([threadId, 0], [threadId, Number.MAX_SAFE_INTEGER]);
        const cursor = await index.openCursor(range, 'prev');
        
        if (cursor && cursor.value.threadId === threadId) {
            return cursor.value.seqIndex;
        }
        
        return -1;
    }

    /**
     * Delete all messages for a thread.
     */
    async clearThread(threadId: number): Promise<void> {
        const db = await this.ensureDb();
        const tx = db.transaction(['messages', 'syncState'], 'readwrite');

        // Delete all messages for this thread
        const msgStore = tx.objectStore('messages');
        const index = msgStore.index('by-thread');
        let cursor = await index.openCursor(threadId);

        while (cursor) {
            await cursor.delete();
            cursor = await cursor.continue();
        }

        // Delete sync state
        await tx.objectStore('syncState').delete(threadId);

        await tx.done;
    }

    // ========== Sync State Operations ==========

    /**
     * Get sync state for a thread.
     * Returns null if thread has never been synced.
     */
    async getSyncState(threadId: number): Promise<ThreadSyncState | null> {
        const db = await this.ensureDb();
        const state = await db.get('syncState', threadId);
        return state ?? null;
    }

    /**
     * Save sync state for a thread.
     */
    async saveSyncState(state: ThreadSyncState): Promise<void> {
        const db = await this.ensureDb();
        await db.put('syncState', state);
    }

    /**
     * Update sync state with new offset values.
     * Creates default state if none exists.
     */
    async updateSyncOffsets(
        threadId: number,
        pulledItemsOffset: number,
        pushedItemsOffset: number,
        lastPulledCommitId: string | null,
        lastPushedCommitId: string | null
    ): Promise<void> {
        const existing = await this.getSyncState(threadId);
        const state: ThreadSyncState = {
            threadId,
            pulledItemsOffset,
            pushedItemsOffset,
            lastPulledCommitId,
            lastPushedCommitId,
            lastSyncTime: Date.now(),
            fragments: existing?.fragments ?? [],
        };
        await this.saveSyncState(state);
    }

    // ========== Fragment Tracking ==========

    /**
     * Add a new message fragment (continuous range).
     * Merges overlapping or adjacent fragments automatically.
     */
    async addFragment(threadId: number, startSeq: number, endSeq: number): Promise<void> {
        const syncState = await this.getSyncState(threadId);
        const fragments = syncState?.fragments ?? [];

        // Add new fragment
        const newFragment: MessageFragment = { startSeq, endSeq };
        fragments.push(newFragment);

        // Merge overlapping/adjacent fragments
        const merged = this.mergeFragments(fragments);

        // Save updated state
        const state: ThreadSyncState = {
            threadId,
            pulledItemsOffset: syncState?.pulledItemsOffset ?? 0,
            pushedItemsOffset: syncState?.pushedItemsOffset ?? 0,
            lastPulledCommitId: syncState?.lastPulledCommitId ?? null,
            lastPushedCommitId: syncState?.lastPushedCommitId ?? null,
            lastSyncTime: Date.now(),
            fragments: merged,
        };
        await this.saveSyncState(state);
    }

    /**
     * Check if a sequence index is within a known fragment.
     */
    async isInFragment(threadId: number, seqIndex: number): Promise<boolean> {
        const syncState = await this.getSyncState(threadId);
        if (!syncState) return false;

        return syncState.fragments.some(f => seqIndex >= f.startSeq && seqIndex <= f.endSeq);
    }

    /**
     * Find gaps between fragments.
     * Returns ranges that are missing from local storage.
     */
    async findGaps(threadId: number, fromSeq: number, toSeq: number): Promise<MessageFragment[]> {
        const syncState = await this.getSyncState(threadId);
        if (!syncState || syncState.fragments.length === 0) {
            return [{ startSeq: fromSeq, endSeq: toSeq }];
        }

        const gaps: MessageFragment[] = [];
        const fragments = [...syncState.fragments].sort((a, b) => a.startSeq - b.startSeq);

        let currentStart = fromSeq;

        for (const fragment of fragments) {
            if (fragment.startSeq > currentStart && fragment.startSeq <= toSeq) {
                gaps.push({
                    startSeq: currentStart,
                    endSeq: Math.min(fragment.startSeq - 1, toSeq),
                });
            }
            currentStart = Math.max(currentStart, fragment.endSeq + 1);
        }

        // Check for gap after the last fragment
        if (currentStart <= toSeq) {
            gaps.push({ startSeq: currentStart, endSeq: toSeq });
        }

        return gaps;
    }

    /**
     * Merge overlapping or adjacent fragments.
     */
    private mergeFragments(fragments: MessageFragment[]): MessageFragment[] {
        if (fragments.length <= 1) return fragments;

        // Sort by start position
        const sorted = [...fragments].sort((a, b) => a.startSeq - b.startSeq);
        const merged: MessageFragment[] = [sorted[0]];

        for (let i = 1; i < sorted.length; i++) {
            const current = sorted[i];
            const last = merged[merged.length - 1];

            // Check if current overlaps or is adjacent to last
            if (current.startSeq <= last.endSeq + 1) {
                // Merge by extending the end
                last.endSeq = Math.max(last.endSeq, current.endSeq);
            } else {
                // No overlap, add as new fragment
                merged.push(current);
            }
        }

        return merged;
    }

    // ========== Utility Methods ==========

    /**
     * Convert stored message to KahlaCommit format.
     */
    private storedToCommit(stored: StoredMessage): KahlaCommit<ChatMessage> {
        return {
            id: stored.commitId,
            item: {
                content: stored.content,
                preview: stored.preview,
                senderId: stored.senderId,
                ats: stored.ats,
            },
            commitTime: new Date(stored.commitTime),
        };
    }

    /**
     * Get all thread IDs that have stored messages.
     */
    async getAllThreadIds(): Promise<number[]> {
        const db = await this.ensureDb();
        const keys = await db.getAllKeys('syncState');
        return keys;
    }

    /**
     * Get storage statistics for debugging.
     */
    async getStats(): Promise<{
        totalMessages: number;
        threadCount: number;
        threads: Array<{ threadId: number; messageCount: number }>;
    }> {
        const db = await this.ensureDb();
        const threadIds = await this.getAllThreadIds();
        
        let totalMessages = 0;
        const threads: Array<{ threadId: number; messageCount: number }> = [];

        for (const threadId of threadIds) {
            const count = await this.getMessageCount(threadId);
            totalMessages += count;
            threads.push({ threadId, messageCount: count });
        }

        return {
            totalMessages,
            threadCount: threadIds.length,
            threads,
        };
    }
}

// Export singleton instance for convenience
export const messageStore = new KahlaMessagesIndexedDBStore();
