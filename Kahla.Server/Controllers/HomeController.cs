using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Handler.Attributes;
using Aiursoft.Handler.Models;
using Aiursoft.Probe.SDK.Services;
using Aiursoft.SDK.Services;
using Aiursoft.WebTools;
using Kahla.SDK.Models;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ServiceLocation _serviceLocation;
        private readonly VersionService _sdkVersion;
        private readonly IConfiguration _configuration;
        private readonly ProbeLocator _probeLocator;
        private readonly List<DomainSettings> _appDomain;

        public HomeController(
            IWebHostEnvironment env,
            ServiceLocation serviceLocation,
            VersionService sdkVersion,
            IOptions<List<DomainSettings>> optionsAccessor,
            IConfiguration configuration,
            ProbeLocator probeLocator)
        {
            _env = env;
            _serviceLocation = serviceLocation;
            _sdkVersion = sdkVersion;
            _configuration = configuration;
            _probeLocator = probeLocator;
            _appDomain = optionsAccessor.Value;
        }

        [APIProduces(typeof(IndexViewModel))]
        public IActionResult Index()
        {
            var model = new IndexViewModel
            {
                Code = ErrorType.Success,
                Mode = _env.EnvironmentName,
                Message = $"Welcome to Aiursoft Kahla API! Running in {_env.EnvironmentName} mode.",
                WikiPath = _serviceLocation.Wiki,
                ServerTime = DateTime.Now,
                UTCTime = DateTime.UtcNow,
                APIVersion = _sdkVersion.GetSDKVersion(),
                VapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"],
                ServerName = _configuration["ServerName"],
                Domain = _appDomain.SingleOrDefault(t => t.Server.Split(':')[0] == Request.Host.Host),
                Probe = _probeLocator,
                AutoAcceptRequests = _configuration["AutoAcceptRequests"] == true.ToString().ToLower()
            };
            // This part of code is not beautiful. Try to resolve it in the future.
            if (model.Domain != null)
            {
                model.Domain = new DomainSettings
                {
                    Server = Request.Scheme + "://" + model.Domain.Server,
                    Client = model.Domain.Client
                };
            }
            return this.Protocol(model);
        }
    }
}
