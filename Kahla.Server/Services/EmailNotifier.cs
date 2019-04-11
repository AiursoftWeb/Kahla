using Aiursoft.Pylon.Services;
using Kahla.Server.Data;
using Kahla.Server.Models;
using Kahla.Server.Models.ApiViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class EmailNotifier : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private IServiceScopeFactory _scopeFactory;

        public EmailNotifier(
            ILogger<EmailNotifier> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email notifier service is starting...");
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(30), TimeSpan.FromHours(23));
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
                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<AiurEmailSender>();
                    var users = await dbContext
                                    .Users
                                    .AsNoTracking()
                                    .ToListAsync();
                    foreach (var user in users.Where(t => t.EmailConfirmed))
                    {
                        int totalUnread = 0, inConversatons = 0, pendingRequests = 0;
                        var list = new List<ContactInfo>();
                        var conversations = await dbContext.MyConversations(user.Id);
                        foreach (var conversation in conversations)
                        {
                            // Ignore conversations muted.
                            if (conversation is GroupConversation currentGroup)
                            {
                                var relation = currentGroup
                                    .Users
                                    .FirstOrDefault(t => t.UserId == user.Id);
                                if (relation.Muted)
                                {
                                    continue;
                                }
                            }
                            var currentUnread = conversation.GetUnReadAmount(user.Id);
                            if (currentUnread > 0)
                            {
                                totalUnread += currentUnread;
                                inConversatons++;
                            }
                        }
                        pendingRequests = await dbContext
                            .Requests
                            .AsNoTracking()
                            .Where(t => t.TargetId == user.Id)
                            .CountAsync(t => t.Completed == false);

                        if (inConversatons > 0 || pendingRequests > 0)
                        {
                            string message =
                                (inConversatons > 0 ? $"<h4>You have {totalUnread} unread message(s) in {inConversatons} conversation(s) from your Kahla friends!<h4>\r\n" : "")
                                +
                                (pendingRequests > 0 ? $"<h4>You have {pendingRequests} pending friend request(s) in Kahla.<h4>\r\n" : "")
                                +
                                $"Click to <a href='{configuration["AppDomain"]}'>Open Kahla Now</a>.";
                            await emailSender.SendEmail(user.Email, "New notifications in Kahla", message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred.");
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email notifier service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
