using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class CompleteRequestAddressModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool Accept { get; set; }
    }
}
