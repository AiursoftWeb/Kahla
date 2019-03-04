using Aiursoft.Pylon;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Mvc;

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
            return this.Protocol(ErrorType.Success, "Welcome to kahla server! View our wiki at: " + _serviceLocation.Wiki);
        }

        public IActionResult Error()
        {
            return this.Protocol(ErrorType.UnknownError, "Kahla server was crashed! Please tell us!");
        }
    }
}
