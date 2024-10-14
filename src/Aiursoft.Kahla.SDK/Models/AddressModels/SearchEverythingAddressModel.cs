using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels
{
    public class SearchEverythingAddressModel
    {
        [MinLength(1)]
        [MaxLength(50)]
        [Required]
        public required string SearchInput { get; init; }

        public int Skip { get; init; }
        
        public int Take { get; init; } = 20;
    }
}
