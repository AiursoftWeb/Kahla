using Aiursoft.Pylon.Interfaces;
using Kahla.SDK.Services;
using System;
using System.Threading.Tasks;

namespace Kahla.EchoBot
{
    public class BotCore : IScopedDependency
    {
        private readonly HomeService _homeService;
        private readonly BotLogger _botLogger;
        private readonly KahlaLocation _kahlaLocation;

        public BotCore(
            HomeService homeService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation)
        {
            _homeService = homeService;
            _botLogger = botLogger;
            _kahlaLocation = kahlaLocation;
        }

        public async Task Run()
        {
            if (!await TestKahlaLive())
            {
                return;
            }
        }

        private async Task<bool> TestKahlaLive()
        {
            try
            {
                _botLogger.LogInfo("Testing Kahla server connection...");
                _botLogger.LogInfo($"Using Kahla Server: {_kahlaLocation}");
                await Task.Delay(1000);
                var index = await _homeService.IndexAsync();
                _botLogger.LogInfo($"Server time: {index.Value}");
                await Task.Delay(200);
                _botLogger.LogSuccess("Success! Your bot have successfully connected with Kahla!");
                return true;
            }
            catch (Exception e)
            {
                _botLogger.LogDanger(e.Message);
                return false;
            }
        }
    }
}
