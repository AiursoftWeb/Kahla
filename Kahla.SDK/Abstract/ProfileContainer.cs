using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Models;

namespace Kahla.SDK.Abstract
{
    public class ProfileContainer : ISingletonDependency
    {
        public KahlaUser Profile { get; set; }
    }
}
