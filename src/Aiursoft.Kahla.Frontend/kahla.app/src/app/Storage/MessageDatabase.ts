import { openDB, DBSchema, IDBPDatabase } from 'idb';

/**
 * Represents a message stored in IndexedDB.
 * Optimized for efficient querying and ordering.
 */
export interface StoredMessage {
    /** Thread/conversation ID */
    threadId: number;
    /** Sequential index for ordering within the thread */
    seqIndex: number;
    /** Original commit UUID from the sync protocol */
    commitId: string;
    /** Message content (JSON string) */
    content: string;
    /** Preview text for message list */
    preview?: string;
    /** Sender's user ID */
    senderId?: string;
    /** Mentioned user IDs */
    ats?: string[];
    /** Commit timestamp (Unix ms) */
    commitTime: number;
}

/**
 * Represents a continuous range of messages.
 * Used to track gaps in message history.
 */
export interface MessageFragment {
    /** Starting sequence index (inclusive) */
    startSeq: number;
    /** Ending sequence index (inclusive) */
    endSeq: number;
}

/**
 * Synchronization state for a thread.
 * Persists the double-pointer protocol state.
 */
export interface ThreadSyncState {
    /** Thread/conversation ID */
    threadId: number;
    /** Number of messages successfully pulled from server */
    pulledItemsOffset: number;
    /** Number of messages successfully pushed to server */
    pushedItemsOffset: number;
    /** Commit ID of the last pulled message */
    lastPulledCommitId: string | null;
    /** Commit ID of the last pushed message */
    lastPushedCommitId: string | null;
    /** Last sync timestamp (Unix ms) */
    lastSyncTime: number;
    /** Tracked message fragments for gap handling */
    fragments: MessageFragment[];
}

/**
 * IndexedDB schema definition for type-safe database operations.
 */
interface KahlaMessageDBSchema extends DBSchema {
    messages: {
        key: [number, number]; // [threadId, seqIndex]
        value: StoredMessage;
        indexes: {
            'by-thread': number;
            'by-commit-id': [number, string];
            'by-seq': [number, number];
        };
    };
    syncState: {
        key: number; // threadId
        value: ThreadSyncState;
    };
}

const DB_NAME = 'kahla-messages';
const DB_VERSION = 1;

let dbInstance: IDBPDatabase<KahlaMessageDBSchema> | null = null;
let dbInitPromise: Promise<IDBPDatabase<KahlaMessageDBSchema>> | null = null;

/**
 * Initialize and get the IndexedDB database instance.
 * Uses singleton pattern to ensure only one connection.
 */
export async function getMessageDatabase(): Promise<IDBPDatabase<KahlaMessageDBSchema>> {
    if (dbInstance) {
        return dbInstance;
    }

    if (dbInitPromise) {
        return dbInitPromise;
    }

    dbInitPromise = openDB<KahlaMessageDBSchema>(DB_NAME, DB_VERSION, {
        upgrade(db, oldVersion, newVersion, transaction) {
            // Version 1: Initial schema
            if (oldVersion < 1) {
                // Create messages store with compound key
                const messagesStore = db.createObjectStore('messages', {
                    keyPath: ['threadId', 'seqIndex'],
                });

                // Index for querying all messages in a thread
                messagesStore.createIndex('by-thread', 'threadId');

                // Index for deduplication by commit ID
                messagesStore.createIndex('by-commit-id', ['threadId', 'commitId'], {
                    unique: true,
                });

                // Index for ordered pagination
                messagesStore.createIndex('by-seq', ['threadId', 'seqIndex']);

                // Create sync state store
                db.createObjectStore('syncState', {
                    keyPath: 'threadId',
                });
            }

            // Future migrations can be added here:
            // if (oldVersion < 2) { ... }
        },
        blocked() {
            console.warn('IndexedDB blocked: Another connection is open');
        },
        blocking() {
            console.warn('IndexedDB blocking: This connection is blocking an upgrade');
            // Close our connection to allow upgrade
            dbInstance?.close();
            dbInstance = null;
        },
        terminated() {
            console.error('IndexedDB connection terminated unexpectedly');
            dbInstance = null;
            dbInitPromise = null;
        },
    });

    dbInstance = await dbInitPromise;
    dbInitPromise = null;

    return dbInstance;
}

/**
 * Close the database connection.
 * Useful for cleanup or forcing reconnection.
 */
export function closeMessageDatabase(): void {
    if (dbInstance) {
        dbInstance.close();
        dbInstance = null;
    }
    dbInitPromise = null;
}

/**
 * Delete the entire database.
 * Use with caution - this removes all stored messages!
 */
export async function deleteMessageDatabase(): Promise<void> {
    closeMessageDatabase();
    await indexedDB.deleteDatabase(DB_NAME);
}

export type { KahlaMessageDBSchema };
