namespace Aiursoft.Kahla.SDK.Events.Abstractions;

public enum EventType
{
    #region Events that means the thread's latest status has changed.

    /// <summary>
    /// Someone sent a new message in a thread that you are in.
    ///
    /// In this case, client should move the thread to the top of the thread list. And show the latest message in the thread.
    /// </summary>
    NewMessage = 0,

    #endregion

    #region Events that means you are no longer in the thread.

    /// <summary>
    /// A thread that you are in has been dissolved.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    ThreadDissolved = 8,

    /// <summary>
    /// You have been kicked from a thread.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    YouBeenKicked = 9,

    /// <summary>
    /// You have left a thread.
    ///
    /// In this case, client should remove the thread from the thread list.
    /// </summary>
    YouLeft = 10,

    #endregion

    // Events that means a new thread has been created.

    #region Events that means a new thread should appear on the thread list.

    /// <summary>
    /// A new thread has been created by you.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    CreateScratched = 16,

    /// <summary>
    /// You have been directly invited to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouDirectJoined = 17,

    /// <summary>
    /// You hard invited someone to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YourHardInviteFinished = 18,

    /// <summary>
    /// You were hard invited to a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouWasHardInvited = 19,

    /// <summary>
    /// You completed a software invite and soft joined a thread.
    ///
    /// In this case, client should add the thread to the thread list.
    /// </summary>
    YouCompletedSoftInvited = 20,

    #endregion
    
    #region The properties of a thread that may change.
    
    /// <summary>
    /// Someone changed the thread's name or avatar.
    /// </summary>
    ThreadPropertyChanged = 32
    #endregion
    
}