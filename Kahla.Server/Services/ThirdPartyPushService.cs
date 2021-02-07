using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Models;
using Kahla.Server.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebPush;

namespace Kahla.Server.Services
{
    public class ThirdPartyPushService : IScopedDependency
    {
        private readonly IConfiguration _configuration;
        private readonly WebPushClient _webPushClient;
        private readonly KahlaDbContext _dbContext;
        private readonly ILogger _logger;

        public ThirdPartyPushService(
            IConfiguration configuration,
            WebPushClient webPushClient,
            ILogger<ThirdPartyPushService> logger,
            KahlaDbContext dbContext)
        {
            _configuration = configuration;
            _webPushClient = webPushClient;
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task PushAsync(IEnumerable<Device> devices, object payload, string triggerEmail = "postermaster@aiursoft.com")
        {
            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];
            // Push to all devices.

            var pushTasks = new ConcurrentBag<Task>();
            foreach (var device in devices)
            {
                async Task PushToDevice()
                {
                    try
                    {
                        var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                        var vapidDetails = new VapidDetails("mailto:" + triggerEmail, vapidPublicKey, vapidPrivateKey);
                        var payloadToken = JsonConvert.SerializeObject(payload, new JsonSerializerSettings()
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        });
                        await _webPushClient.SendNotificationAsync(pushSubscription, payloadToken, vapidDetails);
                    }
                    catch (WebPushException e)
                    {
                        _dbContext.Devices.Remove(device);
                        await _dbContext.SaveChangesAsync();
                        _logger.LogCritical(e, "An WebPush error occured while calling WebPush API: " + e.Message);
                        _logger.LogCritical(e, e.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "An error occured while calling WebPush API: " + e.Message);
                    }
                }
                pushTasks.Add(PushToDevice());
            }
            return Task.WhenAll(pushTasks);
        }
    }
}
