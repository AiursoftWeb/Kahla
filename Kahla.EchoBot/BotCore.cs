using Aiursoft.Pylon.Interfaces;
using Kahla.SDK.Events;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.EchoBot
{
    public class BotCore : IScopedDependency
    {
        private readonly HomeService _homeService;
        private readonly BotLogger _botLogger;
        private readonly KahlaLocation _kahlaLocation;
        private readonly AuthService _authService;
        private readonly AES _aes;

        public BotCore(
            HomeService homeService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation,
            AuthService authService,
            AES aes)
        {
            _homeService = homeService;
            _botLogger = botLogger;
            _kahlaLocation = kahlaLocation;
            _authService = authService;
            _aes = aes;
        }

        public async Task Run()
        {
            if (!await TestKahlaLive())
            {
                return;
            }
            await OpenSignIn();
            var code = await AskCode();
            await SignIn(code);
            await DisplayMyProfile();
            var websocketAddress = await GetWSAddress();
            _botLogger.LogInfo($"Your account channel: {websocketAddress}");
            await MonitorEvents(websocketAddress);
        }

        private async Task<bool> TestKahlaLive()
        {
            try
            {
                _botLogger.LogInfo("Testing Kahla server connection...");
                _botLogger.LogInfo($"Using Kahla Server: {_kahlaLocation}");
                await Task.Delay(1000);
                var index = await _homeService.IndexAsync();
                _botLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!");
                await Task.Delay(200);
                _botLogger.LogInfo($"Server time: {index.Value}");
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

        private async Task SignIn(int code)
        {
            while (true)
            {
                try
                {
                    _botLogger.LogInfo($"Calling sign in API with code: {code}...");
                    var response = await _authService.SignIn(code);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        _botLogger.LogSuccess($"Successfully signed in to your account!");
                        break;
                    }
                }
                catch (WebException)
                {
                    _botLogger.LogDanger($"Invalid code!");
                    code = await AskCode();
                }
            }
        }

        private async Task DisplayMyProfile()
        {
            await Task.Delay(200);
            _botLogger.LogInfo($"Getting account profile...");
            var profile = await _authService.MeAsync();
            await Task.Delay(400);
            var profilestring = JsonConvert.SerializeObject(profile.Value, Formatting.Indented);
            _botLogger.LogInfo($"{profilestring}");
        }

        private async Task<string> GetWSAddress()
        {
            var address = await _authService.InitPusherAsync();
            await Task.Delay(200);
            return address.ServerPath;
        }

        private Task MonitorEvents(string websocketAddress)
        {
            var exitEvent = new ManualResetEvent(false);
            var url = new Uri(websocketAddress);

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
                client.ReconnectionHappened.Subscribe(type => _botLogger.LogWarning($"Reconnection happened, type: {type}"));
                client.MessageReceived.Subscribe(async msg =>
                {
                    var inevent = JsonConvert.DeserializeObject<KahlaEvent>(msg.ToString());
                    if (inevent.Type == EventType.NewMessage)
                    {
                        var typedEvent = JsonConvert.DeserializeObject<NewMessageEvent>(msg.ToString());
                        await OnNewMessageEvent(typedEvent);
                    }
                });
                client.Start();
                exitEvent.WaitOne();
            }
            return Task.CompletedTask;
        }

        private Task OnNewMessageEvent(NewMessageEvent typedEvent)
        {
            string decrypted = _aes.OpenSSLDecrypt(typedEvent.Message.Content, typedEvent.AESKey);
            _botLogger.LogInfo($"On message from sender `{typedEvent.Message.Sender.NickName}`: {decrypted}");
            return Task.CompletedTask;
        }
    }

}
