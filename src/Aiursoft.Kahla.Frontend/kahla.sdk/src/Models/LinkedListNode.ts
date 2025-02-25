import type { IMessageContainerNode } from '../Abstracts/IMessageList.js';

export class LinkedListNode<T> implements IMessageContainerNode<T> {
    public value: T;
    public next: LinkedListNode<T> | null = null;
    public previous: LinkedListNode<T> | null = null;

    constructor(value: T) {
        this.value = value;
    }
}
