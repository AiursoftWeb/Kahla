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
        private readonly AuthService _authService;

        public BotCore(
            HomeService homeService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation,
            AuthService authService)
        {
            _homeService = homeService;
            _botLogger = botLogger;
            _kahlaLocation = kahlaLocation;
            _authService = authService;
        }

        public async Task Run()
        {
            if (!await TestKahlaLive())
            {
                return;
            }
            await OpenSignIn();
            var code = await AskCode();
            _botLogger.LogInfo($"Calling sign in API with code: {code}...");
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
                _botLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!");
                return true;
            }
            catch (Exception e)
            {
                _botLogger.LogDanger(e.Message);
                return false;
            }
        }

        private async Task OpenSignIn()
        {
            _botLogger.LogInfo($"Signing in to Kahla...");
            var address = await _authService.OAuthAsync();
            _botLogger.LogWarning($"Please open your browser to view this address: ");
            address = address.Replace("https%3A%2F%2Fserver.kahla.app%2FAuth%2FAuthResult", "https%3A%2F%2Flocalhost%3A5000");
            _botLogger.LogWarning(address);
            //410969371
        }

        private async Task<int> AskCode()
        {
            int code = -1;
            while (true)
            {
                await Task.Delay(500);
                _botLogger.LogInfo($"Please enther the `code` in the address bar(after signing in):");
                var codeString = Console.ReadLine().Trim();
                if (!int.TryParse(codeString, out code))
                {
                    _botLogger.LogDanger($"Invalid code! Code is a number! You can find it in the address bar after you sign in.");
                    continue;
                }
                break;
            }
            return code;
        }
    }
}
