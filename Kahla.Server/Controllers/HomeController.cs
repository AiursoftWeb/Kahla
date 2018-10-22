using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using System.IO;
using Aiursoft.Pylon.Services;
using Aiursoft.Pylon.Models;

namespace Kahla.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ServiceLocation _serviceLocation;

        public HomeController(ServiceLocation serviceLocation)
        {
            _serviceLocation = serviceLocation;
        }

        public IActionResult Index()
        {
            return this.Protocal(ErrorType.Success, "A song from Github to Microsoft: Don't Let Me Down" + _serviceLocation.Wiki);
        }

        public IActionResult Error()
        {
            return this.Protocal(ErrorType.UnknownError, "Kahla server was crashed! Please tell us!");
        }
    }
}
