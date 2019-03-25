using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class SearchEverythingAddressModel
    {
        [MinLength(1)]
        [Required]
        public string SearchInput { get; set; }

        public int Take { get; set; } = 20;
    }
}
