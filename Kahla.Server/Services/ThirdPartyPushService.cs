using Kahla.Server.Data;
using Kahla.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebPush;

namespace Kahla.Server.Services
{
    public class ThirdPartyPushService
    {
        private readonly KahlaDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly WebPushClient _webPushClient;
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetry;

        public ThirdPartyPushService(
            KahlaDbContext dbContext,
            IConfiguration configuration,
            WebPushClient webPushClient,
            ILogger<ThirdPartyPushService> logger,
            TelemetryClient telemetry)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _webPushClient = webPushClient;
            _logger = logger;
            _telemetry = telemetry;
        }

        public async Task PushAsync(string recieverId, string triggerEmail, string payload)
        {
            var user = _dbContext
                .Users
                .Include(t => t.HisDevices)
                .SingleOrDefault(t => t.Id == recieverId);
            if (user == null)
            {
                throw new ArgumentNullException($"User with id :{recieverId} is null!");
            }
            var devices = user.HisDevices.ToList();
            var devicesToRemove = new List<Device>();

            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];
            // Push to all devices.
            foreach (var device in devices)
            {
                try
                {
                    var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                    var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
                    _logger.LogInformation($"Trying to call WebPush API to push a new event to {user.Id}, Event content is '{payload}', Device ID is {device.Id}");
                    await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch (WebPushException e)
                {
                    _logger.LogCritical(e, "A WebPush error occoured while calling WebPush API: " + e.Message);
                    _telemetry.TrackException(e);
                    devicesToRemove.Add(device);
                }
                catch (Exception e)
                {
                    _telemetry.TrackException(e);
                    _logger.LogCritical(e, "An error occoured while calling WebPush API: " + e.Message);
                }
            }
            _dbContext.Devices.RemoveRange(devicesToRemove);
            await _dbContext.SaveChangesAsync();
        }
    }
}
