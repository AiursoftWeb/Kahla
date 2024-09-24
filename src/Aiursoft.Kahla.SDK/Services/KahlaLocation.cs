using Aiursoft.Kahla.SDK.Models.ApiViewModels;
using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.SDK.Services
{
    public class KahlaLocation : ISingletonDependency
    {
        private readonly BotLogger _botLogger;
        private readonly IServiceScopeFactory _factory;
        private readonly VersionService _versionService;
        private string _kahlaRoot = "https://server.kahla.app";
        public IndexViewModel ServerIndex { get; private set; }

        public KahlaLocation(
            BotLogger botLogger,
            IServiceScopeFactory factory,
            VersionService versionService)
        {
            _botLogger = botLogger;
            _factory = factory;
            _versionService = versionService;
        }

        public override string ToString()
        {
            return _kahlaRoot;
        }

        public async Task RefreshServerConfig()
        {
            _botLogger.LogInfo($"Using Kahla Server: {_kahlaRoot}");
            _botLogger.LogInfo("Testing Kahla server connection...");
            using var scope = _factory.CreateScope();
            var home = scope.ServiceProvider.GetRequiredService<HomeService>();
            ServerIndex = await home.IndexAsync(_kahlaRoot);
            _botLogger.AppendResult(true, 5);
            //_botLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!\r\n");
            _botLogger.LogInfo($"Server time: \t{ServerIndex.UTCTime}\tServer version: \t{ServerIndex.APIVersion}");
            _botLogger.LogInfo($"Local time: \t{DateTime.UtcNow}\tLocal version: \t\t{_versionService.GetSDKVersion()}");
            if (ServerIndex.APIVersion != _versionService.GetSDKVersion())
            {
                _botLogger.AppendResult(false);
                _botLogger.LogDanger("API version don't match! Kahla bot may crash! We strongly suggest checking the API version first!");
            }
            else
            {
                _botLogger.AppendResult(true);
            }
        }

        public async Task UseKahlaServerAsync(string kahlaServerRootPath)
        {
            _kahlaRoot = kahlaServerRootPath;
            await RefreshServerConfig();
        }
    }
}
