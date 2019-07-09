using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace Kahla.Server.Controllers
{
    [LimitPerMin(40)]
    public class HomeController : Controller
    {
        private readonly ServiceLocation _serviceLocation;

        public HomeController(ServiceLocation serviceLocation)
        {
            _serviceLocation = serviceLocation;
        }

        [AiurNoCache]
        public IActionResult Links()
        {
            return PhysicalFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar +  "assetlink.json", "application/json");
        }

        [APIProduces(typeof(AiurValue<DateTime>))]
        public IActionResult Index()
        {
            return Json(new AiurValue<DateTime>(DateTime.UtcNow)
            {
                Code = ErrorType.Success,
                Message = "Welcome to Aiursoft Kahla server! View our wiki at: " + _serviceLocation.Wiki
            });
        }

        public IActionResult Error()
        {
            return this.Protocol(ErrorType.UnknownError, "Kahla server was crashed! Please tell us!");
        }
    }
}
