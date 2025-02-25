import { expect } from 'chai';
import { AsyncSemaphore } from '../src/Tools/AsyncSemaphore.js';

describe('AsyncSemaphore', () => {
    it('should limit concurrent accesses to the max count', async () => {
        const maxCount = 2;
        const semaphore = new AsyncSemaphore(maxCount);
        let runningTasks = 0;
        const runningCounts: number[] = [];

        const tasks = Array(5)
            .fill(0)
            .map(async () => {
                await semaphore.wait();
                runningTasks++;
                runningCounts.push(runningTasks);

                // 模拟异步操作
                await new Promise(resolve => setTimeout(resolve, 100));

                runningTasks--;
                semaphore.release();
            });

        await Promise.all(tasks);

        // 检查同时运行的任务数不超过最大并发数
        expect(Math.max(...runningCounts)).to.be.lessThanOrEqual(maxCount);
    });

    it('Should allow lock acquire correctly', async () => {
        const maxCount = 5;
        const semaphore = new AsyncSemaphore(maxCount);
        let runningTasks = 0;
        const runningCounts: number[] = [];

        // Parallel size: 5

        const tasks = Array(5)
            .fill(0)
            .map(async () => {
                await semaphore.wait();
                runningTasks++;
                runningCounts.push(runningTasks);

                // 模拟异步操作
                await new Promise(resolve => setTimeout(resolve, 100));

                runningTasks--;
                semaphore.release();
            });

        await Promise.all(tasks);

        expect(Math.max(...runningCounts)).to.be.lessThanOrEqual(maxCount);

        // Try extra in & out
        await semaphore.wait();
        await semaphore.wait();
        semaphore.release();
        semaphore.release();
    });
});
