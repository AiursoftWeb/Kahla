using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Services;

namespace Kahla.SDK.Abstract
{
    public class BotFactory : ITransientDependency
    {
        private readonly ConversationService _conversationService;
        private readonly GroupsService _groupsService;
        private readonly FriendshipService _friendshipService;
        private readonly AuthService _authService;
        private readonly HomeService _homeService;
        private readonly KahlaLocation _kahlaLocation;
        private readonly BotLogger _botLogger;
        private readonly VersionService _versionService;
        private readonly SettingsService _settingsService;
        private readonly BotCommander _botCommander;
        private readonly AES _aes;

        public BotFactory(
            ConversationService conversationService,
            GroupsService groupsService,
            FriendshipService friendshipService,
            AuthService authService,
            HomeService homeService,
            KahlaLocation kahlaLocation,
            BotLogger botLogger,
            VersionService versionService,
            SettingsService settingsService,
            BotCommander botCommander,
            AES aes)
        {
            _conversationService = conversationService;
            _groupsService = groupsService;
            _friendshipService = friendshipService;
            _authService = authService;
            _homeService = homeService;
            _kahlaLocation = kahlaLocation;
            _botLogger = botLogger;
            _versionService = versionService;
            _settingsService = settingsService;
            _botCommander = botCommander;
            _aes = aes;
        }

        public BotBase BuildBotProperties(BotBase bareBot)
        {
            bareBot.BotLogger = _botLogger;
            bareBot.AES = _aes;
            bareBot.ConversationService = _conversationService;
            bareBot.FriendshipService = _friendshipService;
            bareBot.HomeService = _homeService;
            bareBot.KahlaLocation = _kahlaLocation;
            bareBot.AuthService = _authService;
            bareBot.VersionService = _versionService;
            bareBot.SettingsService = _settingsService;
            bareBot.GroupsService = _groupsService;
            bareBot.BotCommander = _botCommander.Init(bareBot);
            return bareBot;
        }
    }
}
