using Aiursoft.Canon;

namespace Aiursoft.Kahla.Server.Services;

// ReSharper disable once NotAccessedField.Local
// ReSharper disable once UnusedMember.Local
#pragma warning disable CS9113 // Parameter is unread.
public class KahlaPushService(CanonService canon)
#pragma warning restore CS9113 // Parameter is unread.
{

    // public void NewMemberEvent(KahlaUser receiver, KahlaUser newMember, int conversationId)
    // {
    //     var newMemberEvent = new NewMemberEvent
    //     {
    //         NewMember = newMember,
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, newMemberEvent));
    // }
    //
    // public void SomeoneLeftEvent(KahlaUser receiver, KahlaUser leftMember, int conversationId)
    // {
    //     var someoneLeftEvent = new SomeoneLeftEvent
    //     {
    //         LeftUser = leftMember,
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, someoneLeftEvent));
    // }
    //
    // public void DissolveEvent(KahlaUser receiver, int conversationId)
    // {
    //     var dissolveEvent = new DissolveEvent
    //     {
    //         ConversationId = conversationId
    //     };
    //     _canon.FireAsync<WebSocketPushService>(s => s.PushAsync(receiver, dissolveEvent));
    // }
}
