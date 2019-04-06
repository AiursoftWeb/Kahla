using System.ComponentModel.DataAnnotations;

namespace Kahla.Server.Models.ApiAddressModels
{
    public class UpdateMessageLifeTimeAddressModel
    {
        // Conversation Id
        public int Id { get; set; }

        [Range(5, int.MaxValue)]
        public int NewLifeTime { get; set; }
    }
}
