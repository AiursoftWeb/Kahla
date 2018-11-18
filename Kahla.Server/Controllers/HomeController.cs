using Aiursoft.Pylon;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            return this.Protocal(ErrorType.Success, "Welcome to kahla server! View our wiki at: " + _serviceLocation.Wiki);
        }

        public async Task<IActionResult> Doc()
        {
            var actionsMatches = new List<API>();
            await Task.Delay(0);
            actionsMatches.Clear();
            foreach (var item in Assembly.GetEntryAssembly().GetTypes())
            {
                if (IsController(item))
                {
                    foreach (var method in item.GetMethods())
                    {
                        if (IsAction(method))
                        {
                            var args = new List<Argument>();
                            foreach (var param in method.GetParameters())
                            {
                                if (param.ParameterType.IsClass)
                                {
                                    foreach (var prop in param.ParameterType.GetProperties())
                                    {
                                        args.Add(new Argument { Name = prop.Name });
                                    }
                                }
                                else
                                {
                                    args.Add(new Argument { Name = param.Name });
                                }
                            }
                            var api = new API
                            {
                                ControllerName = item.Name,
                                ActionName = method.Name,
                                IsPost = method.CustomAttributes.Any(t => t.AttributeType == typeof(HttpPostAttribute)).ToString(),
                                Arguments = args
                            };
                            actionsMatches.Add(api);
                        }
                    }
                }
            }
            return Json(actionsMatches);
        }

        public IActionResult Error()
        {
            return this.Protocal(ErrorType.UnknownError, "Kahla server was crashed! Please tell us!");
        }

        private static bool IsController(Type type)
        {
            return
                type.Name.EndsWith("Controller") &&
                type.Namespace.EndsWith("Kahla.Server.Controllers") &&
                type.Name != "Controller" &&
                type.IsSubclassOf(typeof(Controller)) &&
                type.IsPublic;
        }

        private static bool IsAction(MethodInfo method)
        {
            return
                !method.IsAbstract &&
                !method.IsVirtual &&
                !method.IsStatic &&
                !method.IsConstructor &&
                method.Module.ToString() == "Kahla.Server.dll";
        }
    }

    public class API
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string IsPost { get; set; }
        public List<Argument> Arguments { get; set; }
    }

    public class Argument
    {
        public string Name { get; set; }
    }
}
