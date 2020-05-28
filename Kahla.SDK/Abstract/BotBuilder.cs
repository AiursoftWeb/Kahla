﻿using Aiursoft.Scanner;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kahla.SDK.Abstract
{
    public class BotBuilder
    {
        private IServiceProvider _serviceProvider;
        public BotBuilder UseStartUp<T>() where T : IStartUp, new()
        {
            var starter = new T();
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddBots();
            starter.ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            var settings = _serviceProvider.GetService<SettingsService>();
            starter.Configure(settings);
            return this;
        }

        public BotHost<T> Build<T>() where T : BotBase
        {
            using var scope = _serviceProvider.CreateScope();
            var botHost = scope.ServiceProvider.GetRequiredService<BotHost<T>>();
            return botHost;
        }
    }
}