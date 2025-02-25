function traverseFiles(item: FileSystemEntry, base = ''): Promise<[File, string][]> {
    if (item.isFile) {
        return new Promise(resolve => {
            (item as FileSystemFileEntry).file(file => {
                resolve([[file, base + item.name]]);
            });
        });
    } else if (item.isDirectory) {
        return new Promise(resolve => {
            const reader = (item as FileSystemDirectoryEntry).createReader();
            reader.readEntries(async entries => {
                const files = await Promise.all(
                    entries.map(t => traverseFiles(t, base + item.name + '/'))
                );
                resolve(files.flat());
            });
        });
    } else {
        return Promise.resolve([]);
    }
}

export function useFileDropper(
    fileAccept: string[] = ['*/*'],
    onSuccess?: (files: [File, string][]) => void,
    onIncorrectType?: () => void
) {
    async function processList(items?: DataTransferItemList) {
        const files = (
            await Promise.all(
                Array.from(items ?? [])
                    .map(t => t.webkitGetAsEntry() ?? t.getAsFile()) // t.webkitGetAsEntry() not work for copied images.
                    .filter(t => !!t)
                    .map(t => {
                        if (t instanceof File) {
                            return Promise.resolve([[t, t.name] as [File, string]]);
                        }
                        return traverseFiles(t);
                    })
            )
        )
            .flat()
            .filter(t => fileAccept.includes(t[0].type) || fileAccept.includes('*/*'));

        if (files.length > 0) {
            onSuccess?.(files);
        } else {
            onIncorrectType?.();
        }
    }

    return {
        async onDrop(e: DragEvent) {
            e.preventDefault();
            await processList(e.dataTransfer?.items);
        },
        onDragOver(e: DragEvent) {
            e.preventDefault();
        },
        async onPaste(e: ClipboardEvent) {
            await processList(e.clipboardData?.items);
        },
    };
}
