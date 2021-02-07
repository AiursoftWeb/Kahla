using Aiursoft.Scanner.Interfaces;
using Kahla.Bot.Bots;
using Kahla.SDK.Factories;
using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kahla.Bot.Services
{
    public class BackgroundTaskSample : IHostedService, IDisposable, ISingletonDependency
    {
        private Timer _timer;
        private readonly BotLogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundTaskSample(
            BotLogger logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInfo("Task started!");
                using var scope = _scopeFactory.CreateScope();
                var factory = scope.ServiceProvider.GetService<BotFactory<EchoBot>>();
                if (factory != null)
                {
                    var echoBot = factory.ProduceBot();
                    //await echoBot.BroadcastAsync("Broadcast.");
                }
            }
            catch (Exception e)
            {
                _logger.LogDanger(e.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
