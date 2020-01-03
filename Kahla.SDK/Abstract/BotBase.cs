using Aiursoft.XelNaga.Interfaces;
using Kahla.Bot.Services;
using Kahla.SDK.Events;
using Kahla.SDK.Models;
using Kahla.SDK.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Kahla.SDK.Abstract
{
    public abstract class BotBase : ISingletonDependency
    {
        public AES AES;
        public BotLogger BotLogger;
        public ConversationService ConversationService;
        public FriendshipService FriendshipService;
        public AuthService AuthService;
        public HomeService HomeService;
        public KahlaLocation KahlaLocation;
        public VersionService VersionService;
        public SettingsService SettingsService;

        public KahlaUser Profile { get; set; }

        public abstract Task OnBotInit();

        public abstract Task OnFriendRequest(NewFriendRequestEvent arg);

        public abstract Task OnMessage(string inputMessage, NewMessageEvent eventContext);

        public abstract Task OnGroupInvitation(int groupId, NewMessageEvent eventContext);

        public async Task Start()
        {
            var listenTask = await Connect();

            BotLogger.LogSuccess("Bot started! Waitting for commands. Enter 'help' to view available commands.");
            await Task.WhenAll(listenTask, Command());
        }

        public async Task<Task> Connect()
        {
            var server = AskServerAddress();
            SettingsService.Save("ServerAddress", server);
            KahlaLocation.UseKahlaServer(server);
            if (!await TestKahlaLive())
            {
                return Task.CompletedTask;
            }
            if (!await SignedIn())
            {
                await OpenSignIn();
                var code = await AskCode();
                await SignIn(code);
            }
            else
            {
                BotLogger.LogSuccess($"You are already signed in! Welcome!");
            }
            await RefreshUserProfile();
            await OnBotInit();
            var websocketAddress = await GetWSAddress();
            BotLogger.LogInfo($"Listening to your account channel: {websocketAddress}");
            var requests = (await FriendshipService.MyRequestsAsync())
                .Items
                .Where(t => !t.Completed);
            foreach (var request in requests)
            {
                await OnFriendRequest(new NewFriendRequestEvent
                {
                    RequestId = request.Id,
                    Requester = request.Creator,
                    RequesterId = request.CreatorId,
                });
            }
            return MonitorEvents(websocketAddress);
        }

        public string AskServerAddress()
        {
            var cached = SettingsService.Read("ServerAddress") as string;
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return cached;
            }
            BotLogger.LogInfo("Welcome! Please enter the server address of Kahla.");
            BotLogger.LogWarning("\r\nEnter 1 for production\r\nEnter 2 for staging\r\nFor other server, enter like: https://server.kahla.app");
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

        public async Task<bool> TestKahlaLive()
        {
            try
            {
                BotLogger.LogInfo($"Using Kahla Server: {KahlaLocation}");
                BotLogger.LogInfo("Testing Kahla server connection...");
                var index = await HomeService.IndexAsync();
                BotLogger.LogSuccess("Success! Your bot is successfully connected with Kahla!\r\n");
                BotLogger.LogInfo($"Server time: \t\t{index.UTCTime}\tLocal time: \t\t{DateTime.UtcNow}");
                BotLogger.LogInfo($"Server version: \t{index.APIVersion}\t\t\tLocal version: \t{VersionService.GetSDKVersion()}");
                if (index.APIVersion != VersionService.GetSDKVersion())
                {
                    BotLogger.LogDanger("API version don't match! Kahla bot may crash! We strongly suggest checking the API version first!");
                }
                else
                {
                    BotLogger.LogSuccess("API version match!");
                }
                return true;
            }
            catch (Exception e)
            {
                BotLogger.LogDanger(e.Message);
                return false;
            }
        }

        public async Task<bool> SignedIn()
        {
            var status = await AuthService.SignInStatusAsync();
            return status.Value;
        }

        public async Task OpenSignIn()
        {
            BotLogger.LogInfo($"Signing in to Kahla...");
            var address = await AuthService.OAuthAsync();
            BotLogger.LogWarning($"Please open your browser to view this address: ");
            address = address.Split('&')[0] + "&redirect_uri=https%3A%2F%2Flocalhost%3A5000";
            BotLogger.LogWarning(address);
            //410969371
        }

        public async Task<int> AskCode()
        {
            int code;
            while (true)
            {
                await Task.Delay(10);
                BotLogger.LogInfo($"Please enther the `code` in the address bar(after signing in):");
                var codeString = Console.ReadLine().Trim();
                if (!int.TryParse(codeString, out code))
                {
                    BotLogger.LogDanger($"Invalid code! Code is a number! You can find it in the address bar after you sign in.");
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
                    BotLogger.LogInfo($"Calling sign in API with code: {code}...");
                    var response = await AuthService.SignIn(code);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        BotLogger.LogSuccess($"Successfully signed in to your account!");
                        break;
                    }
                }
                catch (WebException)
                {
                    BotLogger.LogDanger($"Invalid code!");
                    code = await AskCode();
                }
            }
        }

        public async Task RefreshUserProfile()
        {
            BotLogger.LogInfo($"Getting account profile...");
            var profile = await AuthService.MeAsync();
            Profile = profile.Value;
        }

        public async Task<string> GetWSAddress()
        {
            var address = await AuthService.InitPusherAsync();
            return address.ServerPath;
        }

        public Task MonitorEvents(string websocketAddress)
        {
            var exitEvent = new ManualResetEvent(false);
            var url = new Uri(websocketAddress);
            var client = new WebsocketClient(url)
            {
                ReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds
            };
            client.ReconnectionHappened.Subscribe(type => BotLogger.LogVerbose($"WebSocket: {type}"));
            client.MessageReceived.Subscribe(OnStargateMessage);
            client.Start();
            return Task.Run(exitEvent.WaitOne);
        }

        public async void OnStargateMessage(ResponseMessage msg)
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
                await OnFriendRequest(typedEvent);
            }
        }

        public async Task OnNewMessageEvent(NewMessageEvent typedEvent)
        {
            string decrypted = AES.OpenSSLDecrypt(typedEvent.Message.Content, typedEvent.AESKey);
            BotLogger.LogInfo($"On message from sender `{typedEvent.Message.Sender.NickName}`: {decrypted}");
            if (decrypted.StartsWith("[group]") && int.TryParse(decrypted.Substring(7), out int groupId))
            {
                await OnGroupInvitation(groupId, typedEvent);
            }
            else
            {
                await OnMessage(decrypted, typedEvent).ConfigureAwait(false);
            }
        }

        public Task CompleteRequest(int requestId, bool accept)
        {
            var text = accept ? "accepted" : "rejected";
            BotLogger.LogWarning($"Friend request with id '{requestId}' was {text}.");
            return FriendshipService.CompleteRequestAsync(requestId, accept);
        }

        public async Task SendMessage(string message, int conversationId, string aesKey)
        {
            var encrypted = AES.OpenSSLEncrypt(message, aesKey);
            await ConversationService.SendMessageAsync(encrypted, conversationId);
        }

        public async Task LogOff()
        {
            await AuthService.LogoffAsync();
        }

        public async Task Command()
        {
            while (true)
            {
                Console.Write($"Bot:\\System\\{Profile.NickName}>");
                var command = Console.ReadLine();
                if (command.Length < 1)
                {
                    continue;
                }
                switch (command.ToLower().Trim()[0])
                {
                    case 'e':
                        Environment.Exit(0);
                        return;
                    case 'a':
                        var conversations = await ConversationService.AllAsync();
                        BotLogger.LogSuccess($"Successfully get all your conversations.");
                        foreach (var conversation in conversations.Items)
                        {
                            BotLogger.LogInfo($"ID:\t{conversation.ConversationId}");
                            BotLogger.LogInfo($"Name:\t{conversation.DisplayName}");
                            BotLogger.LogInfo($"Online:\t{conversation.Online}");
                            BotLogger.LogInfo($"Type:\t{conversation.Discriminator}");
                            BotLogger.LogInfo($"Last:\t{conversation.LatestMessage}");
                            BotLogger.LogInfo($"Time:\t{conversation.LatestMessageTime}");
                            BotLogger.LogInfo($"Unread:\t{conversation.UnReadAmount}\n");
                        }
                        break;
                    case 'c':
                        Console.Clear();
                        break;
                    case 'l':
                        await LogOff();
                        BotLogger.LogWarning($"Successfully log off. Use command:`r` to reconnect.");
                        break;
                    case 'h':
                        BotLogger.LogInfo($"Kahla bot commands:");

                        BotLogger.LogInfo($"\r\nConversation");
                        BotLogger.LogInfo($"\ta\tShow all conversations.");
                        BotLogger.LogInfo($"\ts\tSay something to someone.");
                        BotLogger.LogInfo($"\tb\tBroadcast to all conversations.");
                        BotLogger.LogInfo($"\tc\tClear console.");

                        BotLogger.LogInfo($"\r\nGroup");
                        BotLogger.LogInfo($"\tm\tMute all groups.");
                        BotLogger.LogInfo($"\tu\tUnmute all groups.");

                        BotLogger.LogInfo($"\r\nNetwork");
                        BotLogger.LogInfo($"\tr\tReconnect to Stargate.");
                        BotLogger.LogInfo($"\tl\tLogout.");

                        BotLogger.LogInfo($"\r\nProgram");
                        BotLogger.LogInfo($"\th\tShow help.");
                        BotLogger.LogInfo($"\te\tQuit bot.");
                        break;
                    default:
                        BotLogger.LogDanger($"Unknown command: {command}. Please try command: 'h' for help.");
                        break;
                }
            }
        }
    }
}
