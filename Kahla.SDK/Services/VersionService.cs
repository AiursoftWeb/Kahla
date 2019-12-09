using Aiursoft.Pylon.Interfaces;
using System.Reflection;

namespace Kahla.SDK.Services
{
    public class VersionService : ISingletonDependency
    {
        public string GetSDKVersion()
        {
            var assembly = Assembly
                .GetExecutingAssembly();

            var version = assembly.GetName().Version.ToString().Split('.');

            return $"{version[0]}.{version[1]}.{version[2]}";
        }
    }
}
