export class AsyncSemaphore {
    private _pendingTasksQueue: (() => void)[] = [];
    private _currentCount: number = 0;

    constructor(private _maxCount: number) {}

    public async wait(): Promise<void> {
        if (this._currentCount < this._maxCount) {
            ++this._currentCount;
            return;
        }

        await new Promise<void>(resolve => {
            this._pendingTasksQueue.push(resolve);
        });
    }

    public release(): void {
        if (this._currentCount <= 0) {
            throw new Error('No tasks to release.');
        }

        if (this._pendingTasksQueue.length > 0) {
            const resolve = this._pendingTasksQueue.shift();
            resolve!();
        } else {
            --this._currentCount;
        }
    }
}
