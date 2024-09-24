using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Scanner.Abstractions;

namespace Aiursoft.Kahla.SDK.Abstract
{
    public class ProfileContainer : ISingletonDependency
    {
        public KahlaUser Profile { get; set; }
    }
}
