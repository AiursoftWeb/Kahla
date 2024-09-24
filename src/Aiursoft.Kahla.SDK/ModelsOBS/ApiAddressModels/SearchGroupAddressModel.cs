using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class SearchGroupAddressModel
    {
        [MinLength(3)]
        [Required]
        public string? GroupName { get; set; }
        public int Take { get; set; } = 20;
    }
}
