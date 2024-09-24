using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class SearchEverythingAddressModel
    {
        [MinLength(1)]
        [Required]
        public string? SearchInput { get; set; }

        public int Take { get; set; } = 20;
    }
}
