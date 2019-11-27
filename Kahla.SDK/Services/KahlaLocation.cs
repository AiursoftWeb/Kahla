using Aiursoft.Pylon.Interfaces;

namespace Kahla.SDK.Services
{
    public class KahlaLocation : ISingletonDependency
    {
        private string _kahlaRoot = "https://staging.server.kahla.app";
        public override string ToString()
        {
            return _kahlaRoot;
        }
        public void UseKahlaServer(string kahlaServerRootPath)
        {
            _kahlaRoot = kahlaServerRootPath;
        }
    }
}
