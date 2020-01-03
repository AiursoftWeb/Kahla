using Aiursoft.SDK.Models;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kahla.SDK.Models
{
    public class KahlaUser : AiurUserBase
    {
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
        public bool MakeEmailPublic { get; set; } = true;
        [NotMapped]
        public bool IsMe { get; set; }

        [JsonProperty]
        public int ThemeId { get; set; }
        public bool ShouldSerializeThemeId() => IsMe;

        [JsonProperty]
        public override string Email { get; set; }
        public bool ShouldSerializeEmail() => MakeEmailPublic || IsMe;

        [JsonProperty]
        public bool EnableEmailNotification { get; set; }
        public bool ShouldSerializeEnableEmailNotification() => IsMe;

        [JsonProperty]
        public bool EnableEnterToSendMessage { get; set; } = true;
        public bool ShouldSerializeEnableEnterToSendMessage() => IsMe;

        [JsonProperty]
        [NotMapped]
        public bool IsOnline { get; set; } = false;

        public KahlaUser Build(OnlineJudger judge)
        {
            IsOnline = judge.IsOnline(Id);
            return this;
        }
    }
}
