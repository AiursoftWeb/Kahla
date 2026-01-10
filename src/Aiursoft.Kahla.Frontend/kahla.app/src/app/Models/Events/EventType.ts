export enum KahlaEventType {
    // Events that means the thread's latest status has changed.
    NewMessage = 0,

    // Events that means you are no longer in the thread.
    ThreadDissolved = 8,
    YouBeenKicked = 9,
    YouLeft = 10,

    // Events that means a new thread should appear on the thread list.
    CreateScratched = 16,
    YouDirectJoined = 17,
    YourHardInviteFinished = 18,
    YouWasHardInvited = 19,
    YouCompletedSoftInvited = 20,

    // The properties of a thread that may change.
    ThreadPropertyChanged = 32,
}
