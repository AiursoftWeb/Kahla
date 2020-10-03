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
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ThirdPartyPushService(
            IConfiguration configuration,
            WebPushClient webPushClient,
            ILogger<ThirdPartyPushService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _webPushClient = webPushClient;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task PushAsync(IEnumerable<Device> devices, object payload, string triggerEmail = "postermaster@aiursoft.com")
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KahlaDbContext>();
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
                        dbContext.Devices.Remove(device);
                        await dbContext.SaveChangesAsync();
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
            await Task.WhenAny(
                Task.WhenAll(pushTasks),
                Task.Delay(2000));
        }
    }
}
