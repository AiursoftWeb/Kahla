using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Aiursoft.Pylon.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kahla.Server.Models
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
        public IEnumerable<GroupConversation> GroupsCreated { get; set; }

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
        public int CurrentChannel { get; set; } = -1;
        [JsonIgnore]
        public string ConnectKey { get; set; }
        public bool MakeEmailPublic { get; set; } = true;
        public override string Email { get; set; }
        public bool ShouldSerializeEmail() => MakeEmailPublic;
    }
}