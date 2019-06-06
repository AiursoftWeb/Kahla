using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateGroupAddressModel
    {
        [Required]
        public string GroupName { get; set; }

        public int AvatarKey { get; set; }
    }
}
