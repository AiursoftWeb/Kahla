using System.ComponentModel.DataAnnotations.Schema;
using Aiursoft.Kahla.SDK.Services;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Aiursoft.Kahla.SDK.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KahlaUser : IdentityUser
    {
        [JsonProperty]
        public override string Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        [JsonProperty] public virtual string Bio { get; set; }

        [JsonProperty] public virtual string NickName { get; set; }

        [JsonProperty] public virtual string Sex { get; set; }

        /// <summary>
        ///     SiteName/Path/FileName.extision
        /// </summary>
        [JsonProperty]
        public string IconFilePath { get; set; }

        [JsonProperty] public virtual string PreferedLanguage { get; set; } = "UnSet";

        [JsonProperty] public virtual DateTime AccountCreateTime { get; set; } = DateTime.UtcNow;

        [JsonProperty] public override bool EmailConfirmed { get; set; }

        [NotMapped] public override bool PhoneNumberConfirmed => !string.IsNullOrEmpty(PhoneNumber);
        
        [JsonIgnore]
        [InverseProperty(nameof(PrivateConversation.RequestUser))]
        public IEnumerable<PrivateConversation> Friends { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(PrivateConversation.TargetUser))]
        public IEnumerable<PrivateConversation> OfFriends { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(UserGroupRelation.User))]
        public IEnumerable<UserGroupRelation> GroupsJoined { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(GroupConversation.Owner))]
        public IEnumerable<GroupConversation> GroupsOwned { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Message.Sender))]
        public IEnumerable<Message> MessagesSent { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Report.Trigger))]
        public IEnumerable<Report> Reported { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Report.Target))]
        public IEnumerable<Report> ByReported { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Device.KahlaUser))]
        public IEnumerable<Device> HisDevices { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(At.TargetUser))]
        public IEnumerable<At> ByAts { get; set; }

        public int CurrentChannel { get; set; } = -1;
        public string ConnectKey { get; set; }
        public DateTime LastEmailHimTime { get; set; } = DateTime.MinValue;
        public string EmailReasonInJson { get; set; }

        [JsonProperty]
        public bool MarkEmailPublic { get; set; } = true;
        [NotMapped]
        public bool IsMe { get; set; }

        [JsonProperty]
        public int ThemeId { get; set; }
        public bool ShouldSerializeThemeId() => IsMe;

        [JsonProperty]
        public override string Email { get; set; }
        public bool ShouldSerializeEmail() => MarkEmailPublic || IsMe;

        [JsonProperty]
        public bool EnableEmailNotification { get; set; }
        public bool ShouldSerializeEnableEmailNotification() => IsMe;

        [JsonProperty]
        public bool EnableEnterToSendMessage { get; set; } = true;
        public bool ShouldSerializeEnableEnterToSendMessage() => IsMe;

        [JsonProperty]
        public bool EnableInvisiable { get; set; }
        public bool ShouldSerializeEnableInvisiable() => IsMe;

        [JsonProperty]
        public bool ListInSearchResult { get; set; } = true;
        public bool ShouldSerializeListInSearchResult() => IsMe;
    }
}
