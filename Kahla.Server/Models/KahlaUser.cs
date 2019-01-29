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

        public int CurrentChannel { get; set; } = -1;
        public string ConnectKey { get; set; }
        public bool MakeEmailPublic { get; set; } = true;
        public override string Email { get; set; }
        public bool ShouldSerializeEmail() => MakeEmailPublic;
    }
}