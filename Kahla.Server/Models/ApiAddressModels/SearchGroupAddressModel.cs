using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SearchGroupAddressModel
    {
        [MinLength(3)]
        [Required]
        public string GroupName { get; set; }
        public int Take { get; set; } = 20;
    }
}
