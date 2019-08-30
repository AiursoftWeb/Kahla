using Aiursoft.Pylon.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateGroupPasswordAddressModel
    {
        [Required]
        public string GroupName { get; set; }
        [NoSpace]
        [DataType(DataType.Password)]
        public string NewJoinPassword { get; set; }
    }
}
