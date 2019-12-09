using Aiursoft.Pylon.Interfaces;
using System.Reflection;

namespace Kahla.SDK.Services
{
    public class VersionService : ISingletonDependency
    {
        public string GetSDKVersion()
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyVersionAttribute>()
                .Version;

            return version;
        }
    }
}
