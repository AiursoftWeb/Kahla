import type { ChatMessage } from './Models/ChatMessage.js';
import type { KahlaCommit } from './Models/KahlaCommit.js';
import { LinkedList } from './Models/LinkedList.js';
import { LinkedListNode } from './Models/LinkedListNode.js';
import { AsyncSemaphore } from './Tools/AsyncSemaphore.js';
import { uuid4 } from './Tools/uuid.js';
import { Subject } from 'rxjs';

export class KahlaMessagesMemoryStore {
    public messages: LinkedList<KahlaCommit<ChatMessage>> = new LinkedList();

    public lastPulled: LinkedListNode<KahlaCommit<ChatMessage>> | null = null;
    public pulledItemsOffset: number = 0;

    public lastPushed: LinkedListNode<KahlaCommit<ChatMessage>> | null = null;
    public pushedItemsOffset: number = 0;

    public lock: AsyncSemaphore = new AsyncSemaphore(1);
    public onPointersChanged = new Subject<void>();

    public async commit(message: ChatMessage): Promise<void> {
        const commit: KahlaCommit<ChatMessage> = {
            id: uuid4(),
            item: message,
            commitTime: new Date(),
        };

        await this.commitCommit(commit);
    }

    public async commitCommit(commit: KahlaCommit<ChatMessage>): Promise<void> {
        await this.lock.wait();
        this.messages.addLast(commit);
        this.lock.release();
    }

    public async onPulledMessage(commit: KahlaCommit<ChatMessage>): Promise<void> {
        await this.lock.wait();
        let itemInserted = false;

        if (!this.messages.first) {
            // 消息存储为空，将提交添加为第一条消息
            this.messages.addFirst(commit);
            this.lastPulled = this.messages.first;
            this.pulledItemsOffset++;
        } else {
            const nextNode = this.getNextNodeForPull();
            if (nextNode && nextNode.value.id === commit.id) {
                // 拉取的提交与下一条消息匹配；前进 lastPulled
                this.lastPulled = nextNode;
                this.pulledItemsOffset++;
            } else {
                // 在下一节点之前插入拉取的提交
                if (nextNode) {
                    const node = this.messages.addBefore(nextNode, commit);
                    this.lastPulled = node;
                } else {
                    // 下一节点为空，添加到末尾
                    this.messages.addLast(commit);
                    this.lastPulled = this.messages.last;
                }
                this.pulledItemsOffset++;
                itemInserted = true;
            }
        }

        this.updateLastPushed(itemInserted);
        this.lock.release();
        this.onPointersChanged.next();
    }

    private updateLastPushed(itemInserted: boolean): void {
        if (this.lastPulled && this.lastPulled.previous === this.lastPushed) {
            // lastPulled 节点紧跟在 lastPushed 之后；前进 lastPushed
            this.lastPushed = this.lastPulled;
            this.pushedItemsOffset++;
        } else if (itemInserted) {
            // 在 lastPushed 之前插入了新项目；调整 pushedItemsOffset
            this.pushedItemsOffset++;
        }
    }

    private getNextNodeForPull(): LinkedListNode<KahlaCommit<ChatMessage>> | null {
        if (!this.lastPulled) {
            // 尚未拉取任何消息；从第一条消息开始
            return this.messages.first;
        } else {
            // 从 lastPulled 之后的节点开始
            return this.lastPulled.next;
        }
    }

    public *push(): IterableIterator<KahlaCommit<ChatMessage>> {
        if (!this.messages.first) {
            return;
        }
        let pushBegin = this.lastPushed ? this.lastPushed.next : this.messages.first;
        while (pushBegin) {
            this.lastPushed = pushBegin;
            yield this.lastPushed.value;
            pushBegin = pushBegin.next;
            this.pushedItemsOffset++;
        }
    }

    public getAllMessages(): KahlaCommit<ChatMessage>[] {
        return this.messages.toArray();
    }

    public getAllMessagesEnumerable(): Iterable<KahlaCommit<ChatMessage>> {
        return this.messages;
    }

    public getHead(): KahlaCommit<ChatMessage> | null {
        return this.messages.last ? this.messages.last.value : null;
    }
}
