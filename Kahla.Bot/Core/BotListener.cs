using Aiursoft.Pylon.Interfaces;
using Kahla.Bot.Models;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.Bot.Core
{
    public class BotListener : IScopedDependency
    {
        private readonly HomeService _homeService;
        private readonly BotLogger _botLogger;
        private readonly KahlaLocation _kahlaLocation;
        private readonly AuthService _authService;
        private readonly ConversationService _conversationService;
        private readonly FriendshipService _friendshipService;
        private readonly AES _aes;
        private IBot _bot;

        public BotListener(
            HomeService homeService,
            BotLogger botLogger,
            KahlaLocation kahlaLocation,
            AuthService authService,
            ConversationService conversationService,
            FriendshipService friendshipService,
            AES aes)
        {
            _homeService = homeService;
            _botLogger = botLogger;
            _kahlaLocation = kahlaLocation;
            _authService = authService;
            _conversationService = conversationService;
            _friendshipService = friendshipService;
            _aes = aes;
        }

        public BotListener WithBot(IBot bot)
        {
            _bot = bot;
            return this;
        }

        public async Task<Task> Start()
        {
            if (_bot == null)
            {
                _botLogger.LogDanger("You can't start bot listener without a bot!");
                return Task.CompletedTask;
            }
            var server = AskServerAddress();
            _kahlaLocation.UseKahlaServer(server);
            if (!await TestKahlaLive())
            {
                return Task.CompletedTask;
            }
            await OpenSignIn();
            var code = await AskCode();
            await SignIn(code);
            await DisplayMyProfile();
            var websocketAddress = await GetWSAddress();
            _botLogger.LogInfo($"Your account channel: {websocketAddress}");
            return MonitorEvents(websocketAddress);
        }

        private string AskServerAddress()
        {
            _botLogger.LogInfo("Welcome! Please enter the server address of Kahla.");
            _botLogger.LogWarning("\r\nEnter 1 for production\r\nEnter 2 for staging\r\nFor other server, enter like: https://server.kahla.app");
            var result = Console.ReadLine();
            if (result.Trim() == 1.ToString())
            {
                return "https://server.kahla.app";
            }
            else if (result.Trim() == 2.ToString())
            {
                return "https://staging.server.kahla.app";
            }
            else
            {
                return result;
            }
        }

        private async Task<bool> TestKahlaLive()
        {
            try
            {
                _botLogger.LogInfo($"Using Kahla Server: {_kahlaLocation}");
                await Task.Delay(200);
                _botLogger.LogInfo("Testing Kahla server connection...");
                await Task.Delay(1000);
                var index = await _homeService.IndexAsync();
                _botLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!\r\n");
                await Task.Delay(200);
                _botLogger.LogInfo($"Server time: {index.Value}\tLocal time: {DateTime.UtcNow}");
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
            address = address.Split('&')[0] + "&redirect_uri=https%3A%2F%2Flocalhost%3A5000";
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
            _bot.Profile = profile.Value;
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
            var client = new WebsocketClient(url)
            {
                ReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds
            };
            client.ReconnectionHappened.Subscribe(type => _botLogger.LogVerbose($"WebSocket: {type}"));
            client.MessageReceived.Subscribe(OnStargateMessage);
            client.Start();
            return Task.Run(exitEvent.WaitOne);
        }

        private async void OnStargateMessage(ResponseMessage msg)
        {
            var inevent = JsonConvert.DeserializeObject<KahlaEvent>(msg.ToString());
            if (inevent.Type == EventType.NewMessage)
            {
                var typedEvent = JsonConvert.DeserializeObject<NewMessageEvent>(msg.ToString());
                await OnNewMessageEvent(typedEvent);
            }
            else if (inevent.Type == EventType.NewFriendRequestEvent)
            {
                var typedEvent = JsonConvert.DeserializeObject<NewFriendRequestEvent>(msg.ToString());
                await OnNewFriendRequest(typedEvent);
            }
        }

        private async Task OnNewMessageEvent(NewMessageEvent typedEvent)
        {
            string decrypted = _aes.OpenSSLDecrypt(typedEvent.Message.Content, typedEvent.AESKey);
            _botLogger.LogInfo($"On message from sender `{typedEvent.Message.Sender.NickName}`: {decrypted}");
            string sendBack = await _bot.OnMessage(decrypted, typedEvent);
            if (!string.IsNullOrWhiteSpace(sendBack))
            {
                await Task.Run(async () =>
                {
                    await Task.Delay(700);
                    var encrypted = _aes.OpenSSLEncrypt(sendBack, typedEvent.AESKey);
                    await _conversationService.SendMessageAsync(encrypted, typedEvent.Message.ConversationId);
                }).ConfigureAwait(false);
            }
        }

        private async Task OnNewFriendRequest(NewFriendRequestEvent typedEvent)
        {
            _botLogger.LogWarning($"New friend request from '{typedEvent.Requester.NickName}'!");
            var result = await _bot.OnFriendRequest(typedEvent);
            await _friendshipService.CompleteRequestAsync(typedEvent.RequestId, result);
            var text = result ? "accepted" : "rejrected";
            _botLogger.LogWarning($"Friend request from '{typedEvent.Requester.NickName}' was {text}.");
        }
    }
}
