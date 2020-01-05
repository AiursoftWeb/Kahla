using Aiursoft.XelNaga.Interfaces;
using System.Reflection;

namespace Kahla.SDK.Services
{
    public class VersionService : ISingletonDependency
    {
        public string GetSDKVersion()
        {
            return SDKVersion();
        }

        public static string SDKVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            return $"{version[0]}.{version[1]}.{version[2]}";
        }
    }
}
