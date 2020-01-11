using Aiursoft.XelNaga.Interfaces;
using Kahla.SDK.Services;
using System.Collections.Generic;
using System.Linq;

namespace Kahla.SDK.Abstract
{
    public class BotFactory : IScopedDependency
    {
        private readonly IEnumerable<BotBase> _bots;
        private readonly ConversationService _conversationService;
        private readonly GroupsService _groupsService;
        private readonly FriendshipService _friendshipService;
        private readonly AuthService _authService;
        private readonly HomeService _homeService;
        private readonly KahlaLocation _kahlaLocation;
        private readonly BotLogger _botLogger;
        private readonly VersionService _versionService;
        private readonly SettingsService _settingsService;
        private readonly AES _aes;

        public BotFactory(
            IEnumerable<BotBase> bots,
            ConversationService conversationService,
            GroupsService groupsService,
            FriendshipService friendshipService,
            AuthService authService,
            HomeService homeService,
            KahlaLocation kahlaLocation,
            BotLogger botLogger,
            VersionService versionService,
            SettingsService settingsService,
            AES aes)
        {
            _conversationService = conversationService;
            _groupsService = groupsService;
            _friendshipService = friendshipService;
            _authService = authService;
            _homeService = homeService;
            _kahlaLocation = kahlaLocation;
            _botLogger = botLogger;
            _bots = bots;
            _versionService = versionService;
            _settingsService = settingsService;
            _aes = aes;
        }

        private BotBase BuildBotProperties(BotBase bareBot)
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
            return bareBot;
        }

        public BotBase GetBot<T>() where T : BotBase
        {
            var bot = _bots.ToList().Where(t => t.GetType() == typeof(T)).FirstOrDefault();
            return BuildBotProperties(bot);
        }

        public BotBase SelectBot()
        {
            var bot = BotConfigurer.SelectBot(_bots, _settingsService, _botLogger);
            return BuildBotProperties(bot);
        }
    }
}
