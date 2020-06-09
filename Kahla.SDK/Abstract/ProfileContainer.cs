using Kahla.SDK.Models;

namespace Kahla.SDK.Abstract
{
    public class ProfileContainer<T>  where T : BotBase
    {
        public KahlaUser Profile { get; set; }
    }
}
