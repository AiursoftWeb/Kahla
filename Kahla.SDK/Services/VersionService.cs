using Aiursoft.Scanner.Abstract;
using System.Reflection;

namespace Kahla.SDK.Services
{
    public class VersionService : ISingletonDependency
    {
        public string GetSDKVersion()
        {
            return SDKVersion();
        }

        private static string SDKVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString().Split('.');
            if (version == null)
            {
                return null;
            }
            return $"{version[0]}.{version[1]}.{version[2]}";
        }
    }
}
