export function checkIfChildOf(child: Node, parent: Node): boolean {
    let current = child;
    while (current) {
        if (current === parent) return true;
        current = current.parentNode;
    }
    return false;
}
