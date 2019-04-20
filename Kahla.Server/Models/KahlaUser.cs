using System.Collections.Generic;
using Aiursoft.Pylon.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;

namespace Kahla.Server.Models
{
    public class KahlaUser : AiurUserBase
    {
        [InverseProperty(nameof(PrivateConversation.RequestUser))]
        public IEnumerable<PrivateConversation> Friends { get; set; }

        [InverseProperty(nameof(PrivateConversation.TargetUser))]
        public IEnumerable<PrivateConversation> OfFriends { get; set; }

        [InverseProperty(nameof(UserGroupRelation.User))]
        public IEnumerable<UserGroupRelation> GroupsJoined { get; set; }

        [InverseProperty(nameof(GroupConversation.Owner))]
        public IEnumerable<GroupConversation> GroupsCreated { get; set; }

        [InverseProperty(nameof(Message.Sender))]
        public IEnumerable<Message> MessagesSent { get; set; }

        [InverseProperty(nameof(Report.Trigger))]
        public IEnumerable<Report> Reported { get; set; }

        [InverseProperty(nameof(Report.Target))]
        public IEnumerable<Report> ByReported { get; set; }

        [InverseProperty(nameof(Device.KahlaUser))]
        public IEnumerable<Device> HisDevices { get; set; }

        public int CurrentChannel { get; set; } = -1;
        public string ConnectKey { get; set; }
        public DateTime LastEmailHimTime { get; set; } = DateTime.MinValue;

        [JsonProperty]
        public bool MakeEmailPublic { get; set; } = true;
        [NotMapped]
        public bool IsMe { get; set; }

        [JsonProperty]
        public int ThemeId { get; set; }
        public bool ShouldSerializeThemeId() => IsMe;

        public override string Email { get; set; }
        public bool ShouldSerializeEmail() => MakeEmailPublic || IsMe;

        [JsonProperty]
        public bool EnableEmailNotification { get; set; }
        public bool ShouldSerializeEnableEmailNotification() => IsMe;
    }
}