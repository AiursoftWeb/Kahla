using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.ModelsOBS;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models;

[JsonObject(MemberSerialization.OptIn)]
public class KahlaUser : IdentityUser
{
    [JsonProperty]
    public override string Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    [JsonProperty] public virtual string? Bio { get; set; }
    [JsonProperty] public virtual string? NickName { get; set; }

    /// <summary>
    ///     SiteName/Path/FileName.extision
    /// </summary>
    [JsonProperty]
    public string? IconFilePath { get; set; }

    [JsonProperty] public virtual DateTime AccountCreateTime { get; set; } = DateTime.UtcNow;

    [JsonProperty] public override bool EmailConfirmed { get; set; }
    [JsonProperty] public override string? Email { get; set; }

    [JsonIgnore]
    [InverseProperty(nameof(PrivateConversation.RequestUser))]
    public IEnumerable<PrivateConversation> Friends { get; set; } = new List<PrivateConversation>();

    [JsonIgnore]
    [InverseProperty(nameof(PrivateConversation.TargetUser))]
    public IEnumerable<PrivateConversation> OfFriends { get; set; } = new List<PrivateConversation>();

    [JsonIgnore]
    [InverseProperty(nameof(UserGroupRelation.User))]
    public IEnumerable<UserGroupRelation> GroupsJoined { get; set; } = new List<UserGroupRelation>();

    [JsonIgnore]
    [InverseProperty(nameof(GroupConversation.Owner))]
    public IEnumerable<GroupConversation> GroupsOwned { get; set; } = new List<GroupConversation>();

    [JsonIgnore]
    [InverseProperty(nameof(Message.Sender))]
    public IEnumerable<Message> MessagesSent { get; set; } = new List<Message>();

    [JsonIgnore]
    [InverseProperty(nameof(Report.Trigger))]
    public IEnumerable<Report> Reported { get; set; } = new List<Report>();

    [JsonIgnore]
    [InverseProperty(nameof(Report.Target))]
    public IEnumerable<Report> ByReported { get; set; } = new List<Report>();

    [JsonIgnore]
    [InverseProperty(nameof(Device.KahlaUser))]
    public IEnumerable<Device> HisDevices { get; set; } = new List<Device>();

    // User's settings
    [JsonIgnore] public int ThemeId { get; set; } = 0;

    [JsonIgnore] public bool EnableEmailNotification { get; set; } = true;

    [JsonIgnore] public bool ListInSearchResult { get; set; } = true;
    
    [JsonIgnore] public bool EnableEnterToSendMessage { get; set; } = true;
    
    [JsonIgnore] public bool EnableHideMyOnlineStatus { get; set; }

    // Pusher OTP
    [JsonIgnore] public DateTime PushOtpValidTo { get; set; } = DateTime.MinValue;

    [JsonIgnore] [StringLength(36)] public string? PushOtp { get; set; } // Guid
}