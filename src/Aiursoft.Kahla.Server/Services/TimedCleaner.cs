using Aiursoft.CSTools.Tools;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services
{
    public class TimedCleaner : IHostedService, IDisposable, ISingletonDependency
    {
        private Timer _timer;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;

        public TimedCleaner(
            ILogger<TimedCleaner> logger,
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_env.IsDevelopment() || !EntryExtends.IsProgramEntry())
            {
                _logger.LogInformation("Skip cleaner in development environment");
                return Task.CompletedTask;
            }
            _logger.LogInformation("Timed Background Service is starting");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Cleaner task started!");
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<KahlaDbContext>();
                var hugeConversationMessages = dbContext
                    .Conversations
                    .Where(t => t.Messages.Count() > 20000)
                    .SelectMany(t => t.Messages)
                    .OrderBy(t => t.SendTime)
                    .Take(1000);
                dbContext.Messages.RemoveRange(hugeConversationMessages);
                await dbContext.SaveChangesAsync();

                // try to delete messages too old.
                var outdatedMessages = (await dbContext
                    .Messages
                    .Include(t => t.Conversation)
                    .ToListAsync())
                    .Where(t => DateTime.UtcNow > t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds));
                dbContext.Messages.RemoveRange(outdatedMessages);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to clean up obsolete messages");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
