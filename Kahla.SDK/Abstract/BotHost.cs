using Kahla.SDK.Data;
using Kahla.SDK.Events;
using Kahla.SDK.Factories;
using Kahla.SDK.Services;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.WebSockets;
using Websocket.Client;

namespace Kahla.SDK.Abstract
{
    public class BotHost<T> where T : BotBase
    {
        public BotBase BuildBot => _botFactory.ProduceBot();
        private readonly BotCommander<T> _botCommander;
        private readonly BotLogger _botLogger;
        private readonly SettingsService _settingsService;
        private readonly KahlaLocation _kahlaLocation;
        private readonly FriendshipService _friendshipService;
        private readonly AuthService _authService;
        private readonly EventSyncer<T> _eventSyncer;
        private readonly ProfileContainer _profileContainer;
        private readonly BotFactory<T> _botFactory;
        private readonly IEnumerable<IHostedService> _backgroundJobs;
        private ManualResetEvent _exitEvent;

        public Task ConnectTask = Task.CompletedTask;
        public Task MonitorTask = Task.CompletedTask;
        public Task CommandTask = Task.CompletedTask;

        public BotHost(
            BotCommander<T> botCommander,
            BotLogger botLogger,
            SettingsService settingsService,
            KahlaLocation kahlaLocation,
            FriendshipService friendshipService,
            AuthService authService,
            EventSyncer<T> eventSyncer,
            ProfileContainer profileContainer,
            BotFactory<T> botFactory,
            IEnumerable<IHostedService> backgroundJobs)
        {
            _botCommander = botCommander.InjectHost(this);
            _botLogger = botLogger;
            _settingsService = settingsService;
            _kahlaLocation = kahlaLocation;
            _friendshipService = friendshipService;
            _authService = authService;
            _eventSyncer = eventSyncer;
            _profileContainer = profileContainer;
            _botFactory = botFactory;
            _backgroundJobs = backgroundJobs;
        }

        public async Task Run(bool enableCommander = true, int autoReconnectMax = int.MaxValue)
        {
            int reconnectAttempts = 0;
            await BuildBot.OnBotStarting();
            ConnectTask = Connect((websocketAddress) =>
            {
                MonitorTask = MonitorEvents(websocketAddress);
                if (enableCommander)
                {
                    CommandTask = _botCommander.Command();
                }
            });
            while (
                !CommandTask.IsCompleted ||
                !MonitorTask.IsCompleted ||
                !ConnectTask.IsCompleted)
            {
                if (reconnectAttempts < autoReconnectMax
                    && MonitorTask.IsCompleted
                    && ConnectTask.IsCompleted)
                {
                    if (await SignedIn())
                    {
                        reconnectAttempts++;
                        _botLogger.LogSuccess($"\nTrying to auto reconect! Attempts: {reconnectAttempts}");
                        ConnectTask = Connect((websocketAddress) =>
                        {
                            MonitorTask = MonitorEvents(websocketAddress);
                        });
                    }
                    else
                    {
                        _botLogger.LogDanger("Cannot start reconnecting. Because checking sign in status failed.");
                    }
                }
                await Task.Delay(5000);
            }
        }

        public async Task Connect(Action<string> onGetWebsocket = null)
        {
            _botLogger.LogWarning("Establishing the connection to Kahla...");
            await ReleaseMonitorJob();
            var server = AskServerAddress();
            _settingsService["ServerAddress"] = server;
            try
            {
                await _kahlaLocation.UseKahlaServerAsync(server);
            }
            catch (Exception e)
            {
                _botLogger.LogDanger(e.Message);
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
                _botLogger.LogSuccess("\nYou are already signed in! Welcome!");
            }
            await RefreshUserProfile();
            var websocketAddress = await GetWsAddress();
            // Trigger on request.
            var requests = (await _friendshipService.MyRequestsAsync())
                .Items
                .Where(t => !t.Completed);
            foreach (var request in requests)
            {
                await BuildBot.OnFriendRequest(new NewFriendRequestEvent
                {
                    Request = request
                });
            }
            // Trigger group connected.
            var friends = (await _friendshipService.MineAsync());
            foreach (var group in friends.Groups)
            {
                await BuildBot.OnGroupConnected(group);
            }
            onGetWebsocket?.Invoke(websocketAddress);
            await Task.WhenAll(_backgroundJobs.Select(t => t.StopAsync(CancellationToken.None)));
            await Task.WhenAll(_backgroundJobs.Select(t => t.StartAsync(CancellationToken.None)));
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

        public async Task<bool> SignedIn()
        {
            try
            {
                var status = await _authService.SignInStatusAsync();
                return status.Value;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task OpenSignIn()
        {
            _botLogger.LogInfo("Signing in to Kahla...");
            var address = await _authService.OAuthAsync();
            _botLogger.LogWarning("Please open your browser to view this address: ");
            address = address.Split('&')[0] + "&redirect_uri=https%3A%2F%2Flocalhost%3A5000";
            _botLogger.LogWarning(address);
        }

        public async Task<int> AskCode()
        {
            int code;
            while (true)
            {
                await Task.Delay(10);
                var codeString = _botLogger.ReadLine("Please enther the `code` in the address bar(after signing in):").Trim();
                if (!int.TryParse(codeString, out code))
                {
                    _botLogger.LogDanger("Invalid code! Code is a number! You can find it in the address bar after you sign in.");
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
                    _botLogger.LogInfo("Calling sign in API...");
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
                    _botLogger.LogDanger("Invalid code!");
                    code = await AskCode();
                }
            }
        }

        public async Task RefreshUserProfile()
        {
            try
            {
                _botLogger.LogInfo("Getting account profile...");
                var profile = await _authService.MeAsync();
                _botLogger.AppendResult(true, 6);
                _profileContainer.Profile = profile.Value;
            }
            catch (AiurUnexpectedResponse e)
            {
                _botLogger.AppendResult(false, 6);
                _botLogger.LogDanger(e.Message);
            }
        }

        public async Task<string> GetWsAddress()
        {
            _botLogger.LogInfo("Getting websocket channel...");
            var address = await _authService.InitPusherAsync();
            _botLogger.AppendResult(true, 6);
            return address.ServerPath;
        }

        public async Task MonitorEvents(string websocketAddress)
        {
            if (_exitEvent != null)
            {
                _botLogger.LogDanger("Bot is trying to establish a new connection while there is already a connection.");
                return;
            }
            _exitEvent = new ManualResetEvent(false);

            // Start websocket.
            var url = new Uri(websocketAddress);
            var client = new WebsocketClient(url)
            {
                ReconnectTimeout = TimeSpan.FromDays(1)
            };
            client.ReconnectionHappened.Subscribe(type => _botLogger.LogVerbose($"WebSocket: {type.Type}"));
            var subscription = client.DisconnectionHappened.Subscribe((_) =>
            {
                _botLogger.LogDanger("Websocket connection dropped!");
                _exitEvent?.Set();
                _exitEvent = null;
            });
            await client.Start();

            // log.
            _botLogger.LogInfo("Listening to your account channel.");
            _botLogger.LogVerbose(websocketAddress + "\n");
            _botLogger.AppendResult(true, 9);

            // Post connect event.
            await _eventSyncer.Init(client);
            await BuildBot.OnBotStarted();

            // Pend.
            _exitEvent?.WaitOne();
            subscription.Dispose();
            await client.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
            _botLogger.LogVerbose("Websocket connection disconnected.");
        }

        public async Task ReleaseMonitorJob()
        {
            _exitEvent?.Set();
            _exitEvent = null;
            while (!MonitorTask.IsCompleted)
            {
                await Task.Delay(200);
            }
        }

        public async Task LogOff()
        {
            await _authService.LogoffAsync();
        }
    }
}
