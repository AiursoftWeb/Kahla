using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class SearchEverythingAddressModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        [Required]
        public required string SearchInput { get; set; }

        public int Skip { get; set; }
        
        public int Take { get; set; } = 20;
        
    }
}
