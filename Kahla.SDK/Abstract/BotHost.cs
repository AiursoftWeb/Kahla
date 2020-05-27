using Aiursoft.Handler.Exceptions;
using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Data;
using Kahla.SDK.Events;
using Kahla.SDK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.SDK.Abstract
{
    public class BotHost : IScopedDependency
    {
        public readonly BotBase _bot;
        public readonly SemaphoreSlim ConnectingLock = new SemaphoreSlim(1);
        private readonly BotCommander _botCommander;
        private readonly BotLogger _botLogger;
        private readonly SettingsService _settingsService;
        private readonly KahlaLocation _kahlaLocation;
        private readonly FriendshipService _friendshipService;
        private readonly HomeService _homeService;
        private readonly VersionService _versionService;
        private readonly AuthService _authService;
        private readonly EventSyncer _eventSyncer;
        private ManualResetEvent _exitEvent;

        public BotHost(
            BotBase bot,
            BotCommander botCommander,
            BotLogger botLogger,
            SettingsService settingsService,
            KahlaLocation kahlaLocation,
            FriendshipService friendshipService,
            HomeService homeService,
            VersionService versionService,
            AuthService authService,
            EventSyncer eventSyncer)
        {
            _bot = bot;
            _botCommander = botCommander;
            _botLogger = botLogger;
            _settingsService = settingsService;
            _kahlaLocation = kahlaLocation;
            _friendshipService = friendshipService;
            _homeService = homeService;
            _versionService = versionService;
            _authService = authService;
            _eventSyncer = eventSyncer;
        }

        public async Task Run(bool enableCommander)
        {
            await _bot.OnBotStarting();
            if (enableCommander)
            {
                var _ = Connect().ConfigureAwait(false);
                await _botCommander.Command();
            }
            else
            {
                await Connect();
            }
        }

        public async Task Connect()
        {
            await ConnectingLock.WaitAsync();
            _botLogger.LogWarning("Establishing the connection to Kahla...");
            _exitEvent?.Set();
            _exitEvent = null;
            var server = AskServerAddress();
            _settingsService["ServerAddress"] = server;
            _kahlaLocation.UseKahlaServer(server);
            if (!await TestKahlaLive(server))
            {
                return;
            }
            if (!await SignedIn())
            {
                await OpenSignIn();
                var code = await AskCode();
                await SignIn(code);
            }
            else
            {
                _botLogger.LogSuccess($"\nYou are already signed in! Welcome!");
            }
            await RefreshUserProfile();
            var websocketAddress = await GetWSAddress();
            // Trigger on request.
            var requests = (await _friendshipService.MyRequestsAsync())
                .Items
                .Where(t => !t.Completed);
            foreach (var request in requests)
            {
                await _bot.OnFriendRequest(new NewFriendRequestEvent
                {
                    Request = request
                });
            }
            // Trigger group connected.
            var friends = (await _friendshipService.MineAsync());
            foreach (var group in friends.Groups)
            {
                await _bot.OnGroupConnected(group);
            }
            ConnectingLock.Release();
            await MonitorEvents(websocketAddress, _bot.OnBotStarted);
            return;
        }

        public string AskServerAddress()
        {
            var cached = _settingsService["ServerAddress"] as string;
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return cached;
            }
            _botLogger.LogInfo("Welcome! Please enter the server address of Kahla.");
            var result = _botLogger.ReadLine("\r\nEnter 1 for production\r\nEnter 2 for staging\r\nFor other server, enter like: https://server.kahla.app\r\n");
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

        public async Task<bool> TestKahlaLive(string server)
        {
            try
            {
                _botLogger.LogInfo($"Using Kahla Server: {_kahlaLocation}");
                _botLogger.LogInfo("Testing Kahla server connection...");
                var index = await _homeService.IndexAsync(server);
                _botLogger.AppendResult(true, 5);
                //_botLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!\r\n");
                _botLogger.LogInfo($"Server time: \t{index.UTCTime}\tServer version: \t{index.APIVersion}");
                _botLogger.LogInfo($"Local time: \t{DateTime.UtcNow}\tLocal version: \t\t{_versionService.GetSDKVersion()}");
                if (index.APIVersion != _versionService.GetSDKVersion())
                {
                    _botLogger.AppendResult(false, 1);
                    _botLogger.LogDanger("API version don't match! Kahla bot may crash! We strongly suggest checking the API version first!");
                }
                else
                {
                    _botLogger.AppendResult(true, 1);
                }
                return true;
            }
            catch (Exception e)
            {
                _botLogger.LogDanger(e.Message);
                return false;
            }
        }

        public async Task<bool> SignedIn()
        {
            var status = await _authService.SignInStatusAsync();
            return status.Value;
        }

        public async Task OpenSignIn()
        {
            _botLogger.LogInfo($"Signing in to Kahla...");
            var address = await _authService.OAuthAsync();
            _botLogger.LogWarning($"Please open your browser to view this address: ");
            address = address.Split('&')[0] + "&redirect_uri=https%3A%2F%2Flocalhost%3A5000";
            _botLogger.LogWarning(address);
            //410969371
        }

        public async Task<int> AskCode()
        {
            int code;
            while (true)
            {
                await Task.Delay(10);
                var codeString = _botLogger.ReadLine($"Please enther the `code` in the address bar(after signing in):").Trim();
                if (!int.TryParse(codeString, out code))
                {
                    _botLogger.LogDanger($"Invalid code! Code is a number! You can find it in the address bar after you sign in.");
                    continue;
                }
                break;
            }
            return code;
        }

        public async Task SignIn(int code)
        {
            while (true)
            {
                try
                {
                    _botLogger.LogInfo($"Calling sign in API...");
                    var response = await _authService.SignIn(code);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        _botLogger.AppendResult(true, 7);
                        break;
                    }
                }
                catch (WebException)
                {
                    _botLogger.AppendResult(false, 7);
                    _botLogger.LogDanger($"Invalid code!");
                    code = await AskCode();
                }
            }
        }

        public async Task RefreshUserProfile()
        {
            try
            {
                _botLogger.LogInfo($"Getting account profile...");
                var profile = await _authService.MeAsync();
                _botLogger.AppendResult(true, 6);
                _bot.Profile = profile.Value;
            }
            catch (AiurUnexceptedResponse e)
            {
                _botLogger.AppendResult(false, 6);
                _botLogger.LogDanger(e.Message);
            }
        }

        public async Task<string> GetWSAddress()
        {
            _botLogger.LogInfo($"Getting websocket channel...");
            var address = await _authService.InitPusherAsync();
            _botLogger.AppendResult(true, 6);
            return address.ServerPath;
        }

        public async Task MonitorEvents(string websocketAddress, Func<Task> onConnected)
        {
            bool okToStop = false;
            if (_exitEvent != null)
            {
                _botLogger.LogDanger("Bot is trying to establish a new connection while there is already a connection.");
                return;
            }
            _exitEvent = new ManualResetEvent(false);
            var url = new Uri(websocketAddress);
            var client = new WebsocketClient(url)
            {
                ReconnectTimeout = TimeSpan.FromDays(1)
            };
            client.ReconnectionHappened.Subscribe(type => _botLogger.LogVerbose($"WebSocket: {type.Type}"));
            client.DisconnectionHappened.Subscribe(t =>
            {
                if (!okToStop)
                {
                    okToStop = true;
                    _botLogger.LogDanger("Websocket connection dropped! Auto retry...");
                    var _ = Connect().ConfigureAwait(false);
                }
            });
            await _eventSyncer.Init(client, _bot);
            await client.Start();
            _botLogger.LogInfo($"Listening to your account channel.");
            _botLogger.LogVerbose(websocketAddress + "\n");
            _botLogger.AppendResult(true, 9);
            await onConnected();
            _exitEvent.WaitOne();
            okToStop = true;
            await client.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
            _botLogger.LogVerbose("Websocket connection disconnected.");
        }

        public async Task LogOff()
        {
            _exitEvent?.Set();
            _exitEvent = null;
            await _authService.LogoffAsync();
        }
    }
}
