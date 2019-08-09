using System.ComponentModel.DataAnnotations;
using Aiursoft.Pylon.Attributes;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateGroupAddressModel
    {
        [Required]
        public string GroupName { get; set; }

        [Display(Name = "new group name")]
        [MinLength(3)]
        [MaxLength(25)]
        public string NewName { get; set; }

        public string AvatarPath { get; set; }
    }
}
