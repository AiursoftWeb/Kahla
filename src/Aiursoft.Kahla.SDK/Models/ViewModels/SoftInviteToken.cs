using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class SoftInviteToken
{
    /// <summary>
    /// This token can only be used to join this thread.
    /// </summary>
    public required int ThreadId { get; init; }
    
    /// <summary>
    /// The user who created this token.
    /// </summary>
    public required string InviterId { get; init; }
    
    /// <summary>
    /// Only the invited user can use this token.
    /// </summary>
    public required string InvitedUserId { get; init; }
    
    /// <summary>
    /// Only available before this time.
    /// </summary>
    public required DateTime ExpireTime { get; init; }

    public string SerializeObject()
    {
        return $"tid:{ThreadId},iid:{InviterId},uid:{InvitedUserId},et:{ExpireTime}";   
    }

    public static SoftInviteToken DeserializeObject(string token)
    {
        try
        {
            var parts = token.Split(',');
            var threadId = parts[0].Split(':')[1];
            var inviterId = parts[1].Split(':')[1];
            var invitedUserId = parts[2].Split(':')[1];
            var expireTime = parts[3].Split(':')[1];
            return new SoftInviteToken
            {
                ThreadId = Convert.ToInt32(threadId),
                InviterId = inviterId,
                InvitedUserId = invitedUserId,
                ExpireTime = Convert.ToDateTime(expireTime)
            };
        }
        catch (Exception e)
        {
            throw new AiurServerException(Code.Unauthorized, $"Invalid token format. Inner exception: {e.Message}");
        }
    }
}