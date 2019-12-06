using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models.Status;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Services.ToStatusServer;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kahla.Server.Services
{
    public class EmailNotifier : IHostedService, IDisposable, ISingletonDependency
    {
        private Timer _timer;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppsContainer _appsContainer;

        public EmailNotifier(
            ILogger<EmailNotifier> logger,
            IServiceScopeFactory scopeFactory,
            AppsContainer appsContainer)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _appsContainer = appsContainer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email notifier service is starting...");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(100), TimeSpan.FromMinutes(25));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Email notifier task started!");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<KahlaDbContext>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<AiurEmailSender>();
                    var timeLimit = DateTime.UtcNow - TimeSpan.FromHours(23);
                    var users = await dbContext
                                    .Users
                                    .Where(t => t.EmailConfirmed)
                                    .Where(t => t.EnableEmailNotification)
                                    // Only for users who did not send email for a long time.
                                    .Where(t => t.LastEmailHimTime < timeLimit)
                                    .ToListAsync();
                    foreach (var user in users)
                    {
                        _logger.LogInformation($"Building email for user: {user.NickName}...");
                        var emailMessage = await BuildEmail(user, dbContext, configuration["EmailAppDomain"]);
                        if (string.IsNullOrWhiteSpace(emailMessage))
                        {
                            _logger.LogInformation($"User: {user.NickName}'s Email is empty. Skip.");
                            continue;
                        }
                        _logger.LogInformation($"Sending email to user: {user.NickName}.");
                        await emailSender.SendEmail("Kahla Notification", user.Email, "New notifications in Kahla", emailMessage);
                        user.LastEmailHimTime = DateTime.UtcNow;
                        dbContext.Update(user);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogCritical(ex, ex.Message);
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
                        var accessToken = await _appsContainer.AccessToken();
                        await eventService.LogAsync(accessToken, ex.Message, ex.StackTrace, EventLevel.Exception);
                    }
                }
                catch { }
            }
        }

        public string AppendS(int count)
        {
            return count >= 2 ? "s" : string.Empty;
        }

        public async Task<string> BuildEmail(KahlaUser user, KahlaDbContext dbContext, string domain)
        {
            int totalUnread = 0, inConversations = 0;
            var conversations = await dbContext.MyContacts(user.Id).ToListAsync();
            var msg = new StringBuilder();
            foreach (var contact in conversations)
            {
                // Ignore conversations muted.
                if (contact.Discriminator == nameof(GroupConversation))
                {
                    if (contact.Muted)
                    {
                        continue;
                    }
                }
                var currentUnread = contact.UnReadAmount;
                if (currentUnread <= 0) continue;

                totalUnread += currentUnread;
                inConversations++;
                if (inConversations == 20)
                {
                    msg.AppendLine("<li>Some conversations haven't been displayed because there are too many items.</li>");
                }
                else if (inConversations > 20)
                {
                    // append nothing to avoid email too large.
                }
                else
                {
                    msg.AppendLine($"<li>{currentUnread} unread message{AppendS(currentUnread)} in {(contact.Discriminator == nameof(GroupConversation) ? "group" : "friend")} <a href=\"{domain}/talking/{contact.ConversationId}\">{contact.DisplayName}</a>.</li>");
                }
            }
            var pendingRequests = await dbContext
                .Requests
                .AsNoTracking()
                .Where(t => t.TargetId == user.Id)
                .CountAsync(t => t.Completed == false);

            if (inConversations > 0 || pendingRequests > 0)
            {
                if (inConversations > 0)
                {
                    msg.Insert(0,
                        $"<h4>You have {totalUnread} unread message{AppendS(totalUnread)} in {inConversations} conversation{AppendS(inConversations)} from your Kahla friends!<h4>\r\n<ul>\r\n");
                    msg.AppendLine("</ul>");
                }

                if (pendingRequests > 0)
                {
                    msg.AppendLine($"<h4>You have {pendingRequests} pending friend request{AppendS(pendingRequests)} in Kahla.<h4>");
                }

                msg.AppendLine($"Click to <a href='{domain}'>Open Kahla Now</a>.");
                msg.AppendLine($"<br><p>Click <a href='{domain}/advanced-setting'>here</a> to unsubscribe all notifications.</p>");
                return msg.ToString();
            }
            return string.Empty;
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
