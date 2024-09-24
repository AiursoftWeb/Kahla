using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class UpdateGroupAddressModel
    {
        [Required]
        public string? GroupName { get; set; }

        [Display(Name = "new group name")]
        [MinLength(3)]
        [MaxLength(25)]
        public string? NewName { get; set; }

        public string? AvatarPath { get; set; }

        public bool ListInSearchResult { get; set; }
    }
}
