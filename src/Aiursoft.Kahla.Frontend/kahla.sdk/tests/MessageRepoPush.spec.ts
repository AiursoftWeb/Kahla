import { expect } from 'chai';
import type { ChatMessage } from '../src/Models/ChatMessage.js';
import { KahlaMessagesMemoryStore } from '../src/KahlaMessagesMemoryStore.js';

describe('Message Repo Push', () => {
    it('Test empty push', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();
        const emptyPush = Array.from(await messagesStore.push());
        expect(emptyPush.length).to.equal(0);

        const emptyPushAgain = Array.from(await messagesStore.push());
        expect(emptyPushAgain.length).to.equal(0);
    });

    it('Test push commits', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();
        const message: ChatMessage = {
            content: 'Hello, world!',
        };
        await messagesStore.commit(message);

        const pushed = Array.from(await messagesStore.push());
        expect(pushed.length).to.equal(1);
    });

    it('Test push 3 commits then 2 commits', async () => {
        const messagesStore = new KahlaMessagesMemoryStore();
        await messagesStore.commit({ content: 'message 1' });
        await messagesStore.commit({ content: 'message 2' });
        await messagesStore.commit({ content: 'message 3' });

        const initialPush = Array.from(await messagesStore.push());
        expect(initialPush.length).to.equal(3);
        for (let i = 0; i < 3; i++) {
            expect(initialPush[i].item.content).to.equal(`message ${i + 1}`);
        }

        await messagesStore.commit({ content: 'message 4' });
        await messagesStore.commit({ content: 'message 5' });

        const secondPush = Array.from(await messagesStore.push());
        expect(secondPush.length).to.equal(2);
        for (let i = 0; i < 2; i++) {
            expect(secondPush[i].item.content).to.equal(`message ${i + 4}`);
        }
    });
});
