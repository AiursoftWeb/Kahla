using System.ComponentModel.DataAnnotations;
using Aiursoft.Pylon.Attributes;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateGroupAddressModel
    {
        [Required]
        public string GroupName { get; set; }

        [MinLength(3)]
        [MaxLength(25)]
        [NoSpace]
        public string NewName { get; set; }

        public int? AvatarKey { get; set; }
    }
}
