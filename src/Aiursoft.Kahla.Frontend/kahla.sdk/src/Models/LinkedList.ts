import { Subject } from 'rxjs';
import { LinkedListNode } from './LinkedListNode.js';
import type { IMessageContainer } from '../Abstracts/IMessageList.js';

export class LinkedList<T> implements IMessageContainer<T, LinkedListNode<T>> {
    public first: LinkedListNode<T> | null = null;
    public last: LinkedListNode<T> | null = null;

    public onChange = new Subject<{
        type: 'addFirst' | 'addLast' | 'addBefore';
        refNode?: LinkedListNode<T>;
        newNode: LinkedListNode<T>;
    }>();

    addFirst(item: T): LinkedListNode<T> {
        const node = new LinkedListNode(item);
        if (!this.first) {
            this.first = this.last = node;
        } else {
            node.next = this.first;
            this.first.previous = node;
            this.first = node;
        }

        this.onChange.next({ type: 'addFirst', newNode: node });
        return node;
    }

    addLast(item: T): LinkedListNode<T> {
        const node = new LinkedListNode(item);
        if (!this.last) {
            this.first = this.last = node;
        } else {
            node.previous = this.last;
            this.last.next = node;
            this.last = node;
        }

        this.onChange.next({ type: 'addLast', newNode: node });
        return node;
    }

    addBefore(node: LinkedListNode<T>, newItem: T): LinkedListNode<T> {
        const newNode = new LinkedListNode(newItem);
        newNode.next = node;
        newNode.previous = node.previous;
        if (node.previous) {
            node.previous.next = newNode;
        } else {
            this.first = newNode;
        }
        node.previous = newNode;

        this.onChange.next({ type: 'addBefore', refNode: node, newNode });
        return newNode;
    }

    toArray(): T[] {
        const result: T[] = [];
        let current = this.first;
        while (current) {
            result.push(current.value);
            current = current.next;
        }
        return result;
    }

    [Symbol.iterator](): Iterator<T> {
        let current = this.first;
        return {
            next(): IteratorResult<T> {
                if (current) {
                    const value = current.value;
                    current = current.next;
                    return { value, done: false };
                } else {
                    return { value: undefined, done: true };
                }
            },
        };
    }
}
