using Kahla.Server.Data;
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

        public ThirdPartyPushService(
            KahlaDbContext dbContext,
            IConfiguration configuration,
            WebPushClient webPushClient,
            ILogger<ThirdPartyPushService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _webPushClient = webPushClient;
            _logger = logger;
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
            var devices = user.HisDevices;

            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];
            // Push to all devices.
            foreach (var device in devices)
            {
                var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
                _logger.LogInformation($"Trying to call WebPush API to push a new event to {user.Id}, Event content is '{payload}', Device ID is {device.Id}");
                try
                {
                    await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "An error occoured while calling WebPush API: " + e.Message);
                    // Try remove this device.
                    _dbContext.Devices.Remove(device);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
