import { expect } from 'chai';
import { KahlaMessagesMemoryStore } from '../src/KahlaMessagesMemoryStore.js';
import type { ChatMessage } from '../src/Models/ChatMessage.js';
import type { KahlaCommit } from '../src/Models/KahlaCommit.js';
import { uuid4 } from '../src/Tools/uuid.js';
describe('Message Repo Pull', () => {
    it('Test pull existing messages', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: ChatMessage[] = [
            { content: 'Local message 1' },
            { content: 'Local message 2' },
            { content: 'Local message 3' },
        ];

        for (const message of localMessages) {
            await messagesStore.commit(message);
        }

        // 推送本地消息
        const initialPush = Array.from(messagesStore.push());
        expect(initialPush.length).to.equal(3);

        // 模拟拉取已存在的消息（ID匹配）
        const pulledCommits = initialPush; // 模拟相同的提交被拉取
        for (const commit of pulledCommits) {
            await messagesStore.onPulledMessage(commit);
        }

        // 确保没有创建重复项，指针正确前进
        const allMessages = messagesStore.getAllMessagesEnumerable();
        expect(Array.from(allMessages).length).to.equal(3);
        expect(messagesStore.getHead()?.item.content).to.equal('Local message 3');
        expect(messagesStore.lastPulled?.value.item.content).to.equal('Local message 3');
    });

    it('Test pull new messages', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: ChatMessage[] = [
            { content: 'Local message 1' },
            { content: 'Local message 2' },
            { content: 'Local message 3' },
        ];

        for (const message of localMessages) {
            await messagesStore.commit(message);
        }

        // 推送本地消息
        const initialPush = Array.from(messagesStore.push());
        expect(initialPush.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);

        // 模拟拉取新的消息
        const newPulledCommits: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Pulled message 4' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Pulled message 5' },
                commitTime: new Date(),
            },
        ];

        for (const commit of newPulledCommits) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(5);
        expect(allMessages[0].item.content).to.equal('Pulled message 4');
        expect(allMessages[1].item.content).to.equal('Pulled message 5');
        expect(allMessages[2].item.content).to.equal('Local message 1');
        expect(allMessages[3].item.content).to.equal('Local message 2');
        expect(allMessages[4].item.content).to.equal('Local message 3');

        expect(messagesStore.lastPulled?.value.item.content).to.equal('Pulled message 5');
        expect(messagesStore.lastPushed?.value.item.content).to.equal('Local message 3');
        expect(messagesStore.pulledItemsOffset).to.equal(2);
        expect(messagesStore.pushedItemsOffset).to.equal(5);
    });

    it('Test commit push and pull will consolidate', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 1' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 2' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 3' },
                commitTime: new Date(),
            },
        ];

        for (const message of localMessages) {
            await messagesStore.commitCommit(message);
        }

        const initialPush = Array.from(messagesStore.push());
        expect(initialPush.length).to.equal(3);

        for (const commit of localMessages) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);
        expect(messagesStore.pulledItemsOffset).to.equal(3);
    });

    it('Test commit pull and push get nothing', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 1' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 2' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 3' },
                commitTime: new Date(),
            },
        ];

        for (const message of localMessages) {
            await messagesStore.commitCommit(message);
        }

        for (const commit of localMessages) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);
        expect(messagesStore.pulledItemsOffset).to.equal(3);

        const pushed = Array.from(messagesStore.push());
        expect(pushed.length).to.equal(0);
    });

    it('Test pull twice', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 1' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 2' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 3' },
                commitTime: new Date(),
            },
        ];

        for (const commit of localMessages) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);
        expect(messagesStore.pulledItemsOffset).to.equal(3);

        const pushed = Array.from(messagesStore.push());
        expect(pushed.length).to.equal(0);

        const localMessages2: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 4' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 5' },
                commitTime: new Date(),
            },
        ];

        for (const commit of localMessages2) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages2 = messagesStore.getAllMessages();
        expect(allMessages2.length).to.equal(5);
        expect(messagesStore.pushedItemsOffset).to.equal(5);
        expect(messagesStore.pulledItemsOffset).to.equal(5);
    });

    it('Test pull while pushing', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 1' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 2' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 3' },
                commitTime: new Date(),
            },
        ];

        for (const commit of localMessages) {
            await messagesStore.commitCommit(commit);
        }

        for (const commit of messagesStore.push()) {
            await messagesStore.onPulledMessage(commit);
        }

        const allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);
        expect(messagesStore.pulledItemsOffset).to.equal(3);
    });

    it('Test merge', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();

        // 提交本地消息，ID为1，2，3
        const localMessages: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 1' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 2' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 3' },
                commitTime: new Date(),
            },
        ];

        for (const commit of localMessages) {
            await messagesStore.commitCommit(commit);
        }

        for (const commit of messagesStore.push()) {
            await messagesStore.onPulledMessage(commit);
        }

        // 所有内容已同步
        let allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(3);
        expect(messagesStore.pushedItemsOffset).to.equal(3);
        expect(messagesStore.pulledItemsOffset).to.equal(3);

        // 提交6、7、8
        const localMessages2: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Local message 6' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 7' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Local message 8' },
                commitTime: new Date(),
            },
        ];

        for (const commit of localMessages2) {
            await messagesStore.commitCommit(commit);
        }

        // 拉取4、5、6、7、8、9
        const toBePulled: KahlaCommit<ChatMessage>[] = [
            {
                id: uuid4(),
                item: { content: 'Pulled message 4' },
                commitTime: new Date(),
            },
            {
                id: uuid4(),
                item: { content: 'Pulled message 5' },
                commitTime: new Date(),
            },
            localMessages2[0],
            localMessages2[1],
            localMessages2[2],
            {
                id: uuid4(),
                item: { content: 'Pulled message 9' },
                commitTime: new Date(),
            },
        ];

        for (const commit of toBePulled) {
            await messagesStore.onPulledMessage(commit);
        }

        // 最终消息序列：1,2,3,4,5,6,7,8,9
        // 已拉取到9，因此 pushedItemsOffset 应为9
        allMessages = messagesStore.getAllMessages();
        expect(allMessages.length).to.equal(9);
        expect(messagesStore.pushedItemsOffset).to.equal(9);
        expect(messagesStore.pulledItemsOffset).to.equal(9);

        // 推送应不做任何操作
        const pushed = Array.from(messagesStore.push());
        expect(pushed.length).to.equal(0);
    });

    it('test pull 1 commit then push 1 commit', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();
        await messagesStore.onPulledMessage({
            id: uuid4(),
            item: { content: 'Pulled message 1' },
            commitTime: new Date(),
        });

        await messagesStore.commit({
            content: 'Local message 1',
        });

        const pushed = [...messagesStore.push()];
        expect(pushed).length(1);
        expect(pushed[0].item.content).to.equal('Local message 1');
    });
});
