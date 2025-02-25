export interface IMessageContainerNode<T> {
    next: IMessageContainerNode<T> | null;
    previous: IMessageContainerNode<T> | null;
    value: T;
}

export interface IMessageContainer<T, TNode extends IMessageContainerNode<T>> extends Iterable<T> {
    first: TNode | null;
    last: TNode | null;

    addFirst(item: T): TNode;
    addLast(item: T): TNode;
    addBefore(refNode: TNode, newItem: T): TNode;
    toArray(): T[];
}
