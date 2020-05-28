﻿using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Kahla.SDK.Data;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kahla.SDK.Factories
{
    public class BotFactory<T> : IScopedDependency where T : BotBase
    {
        private readonly IServiceScopeFactory _scopeFactory;


        public BotFactory(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public T ProduceBot()
        {
            using var scope = _scopeFactory.CreateScope();
            var conversationService = scope.ServiceProvider.GetRequiredService<ConversationService>();
            var groupsService = scope.ServiceProvider.GetRequiredService<GroupsService>();
            var friendshipService = scope.ServiceProvider.GetRequiredService<FriendshipService>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            var homeService = scope.ServiceProvider.GetRequiredService<HomeService>();
            var kahlaLocation = scope.ServiceProvider.GetRequiredService<KahlaLocation>();
            var botLogger = scope.ServiceProvider.GetRequiredService<BotLogger>();
            var versionService = scope.ServiceProvider.GetRequiredService<VersionService>();
            var settingsService = scope.ServiceProvider.GetRequiredService<SettingsService>();
            var eventSyncer = scope.ServiceProvider.GetRequiredService<EventSyncer>();
            var storageService = scope.ServiceProvider.GetRequiredService<StorageService>();
            var aes = scope.ServiceProvider.GetRequiredService<AES>();
            var bot = scope.ServiceProvider.GetRequiredService<T>();
            bot.ConversationService = conversationService;
            bot.GroupsService = groupsService;
            bot.FriendshipService = friendshipService;
            bot.AuthService = authService;
            bot.HomeService = homeService;
            bot.KahlaLocation = kahlaLocation;
            bot.BotLogger = botLogger;
            bot.VersionService = versionService;
            bot.SettingsService = settingsService;
            bot.EventSyncer = eventSyncer;
            bot.StorageService = storageService;
            bot.AES = aes;
            return bot;
        }
    }
}