using Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class TimedCleaner : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private IServiceScopeFactory _scopeFactory;

        public TimedCleaner(
            ILogger<TimedCleaner> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Cleaner task started!");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KahlaDbContext>();
                    var hugeConversationMessages = dbContext
                        .Conversations
                        .Where(t => t.Messages.Count() > 20000)
                        .SelectMany(t => t.Messages)
                        .OrderBy(t => t.SendTime)
                        .Take(1000);
                    dbContext.Messages.RemoveRange(hugeConversationMessages);
                    await dbContext.SaveChangesAsync();

                    // try delete messages too old.
                    var outdatedMessages = (await dbContext
                        .Messages
                        .Include(t => t.Conversation)
                        .ToListAsync())
                        .Where(t => DateTime.UtcNow > t.SendTime + TimeSpan.FromSeconds(t.Conversation.MaxLiveSeconds));
                    dbContext.Messages.RemoveRange(outdatedMessages);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
