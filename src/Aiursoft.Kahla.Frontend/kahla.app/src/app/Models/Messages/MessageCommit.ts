export interface MessageCommit {
    content: string;
    preview?: string;
    senderId?: string;
    ats: string[]; // GUID[]
}
