using Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public ThirdPartyPushService(
            KahlaDbContext dbContext,
            IConfiguration configuration,
            WebPushClient webPushClient)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _webPushClient = webPushClient;
        }

        public async Task PushAsync(string recieverId, string triggerEmail, string payload)
        {
            var user = _dbContext
                .Users
                .Include(t=>t.HisDevices)
                .SingleOrDefault(t=>t.Id == recieverId);
            if(user == null)
            {
                throw new ArgumentNullException($"User with id :{recieverId} is null!");
            }
            var devices = user.HisDevices;

            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];

            foreach (var device in devices)
            {
                var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
                await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            }
        }
    }
}
