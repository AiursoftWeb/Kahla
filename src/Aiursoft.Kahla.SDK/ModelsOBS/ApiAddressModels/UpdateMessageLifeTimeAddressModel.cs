using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.ModelsOBS.ApiAddressModels
{
    public class UpdateMessageLifeTimeAddressModel
    {
        // Conversation Id
        public int Id { get; set; }

        [Range(5, int.MaxValue)]
        public int NewLifeTime { get; set; }
    }
}
