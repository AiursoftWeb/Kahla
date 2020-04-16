using Aiursoft.SDKTools.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kahla.SDK.Models.ApiAddressModels
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
