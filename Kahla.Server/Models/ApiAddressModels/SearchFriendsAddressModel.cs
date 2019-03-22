using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SearchFriendsAddressModel
    {
        [MinLength(1)]
        [Required]
        public string NickName { get; set; }

        public int Take { get; set; } = 20;
    }
}
