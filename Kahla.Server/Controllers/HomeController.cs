using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            foreach (var controller in Assembly.GetEntryAssembly().GetTypes())
            {
                if (IsController(controller))
                {
                    foreach (var method in controller.GetMethods())
                    {
                        if (IsAction(method))
                        {
                            var args = new List<Argument>();
                            foreach (var param in method.GetParameters())
                            {
                                if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
                                {
                                    foreach (var prop in param.ParameterType.GetProperties())
                                    {
                                        args.Add(new Argument
                                        {
                                            Name = prop.Name,
                                            Required = JudgeRequired(prop.PropertyType, prop.CustomAttributes),
                                            Type = ConvertTypeToArgumentType(prop.PropertyType)
                                        });
                                    }
                                }
                                else
                                {
                                    args.Add(new Argument
                                    {
                                        Name = param.Name,
                                        Required = param.HasDefaultValue ? false : JudgeRequired(param.ParameterType, param.CustomAttributes),
                                        Type = ConvertTypeToArgumentType(param.ParameterType)
                                    });
                                }
                            }
                            var api = new API
                            {
                                ControllerName = controller.Name,
                                ActionName = method.Name,
                                IsPost = method.CustomAttributes.Any(t => t.AttributeType == typeof(HttpPostAttribute)),
                                Arguments = args,
                                AuthRequired =
                                    method.CustomAttributes.Any(t => t.AttributeType == typeof(AiurForceAuth)) ||
                                    controller.CustomAttributes.Any(t => t.AttributeType == typeof(AiurForceAuth))
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

        private ArgumentType ConvertTypeToArgumentType(Type t)
        {
            return
                t == typeof(int) ? ArgumentType.number :
                t == typeof(int?) ? ArgumentType.number :
                t == typeof(string) ? ArgumentType.text :
                t == typeof(DateTime) ? ArgumentType.datetime :
                t == typeof(DateTime?) ? ArgumentType.datetime :
                t == typeof(bool) ? ArgumentType.boolean :
                t == typeof(bool?) ? ArgumentType.boolean :
                ArgumentType.unknown;
        }

        private bool JudgeRequired(Type source, IEnumerable<CustomAttributeData> attributes)
        {
            if (attributes.Any(t => t.AttributeType == typeof(RequiredAttribute)))
            {
                return true;
            }
            return
                source == typeof(int) ? true :
                source == typeof(DateTime) ? true :
                source == typeof(bool) ? true :
                false;
        }
    }

    public class API
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool AuthRequired { get; set; }
        public bool IsPost { get; set; }
        public List<Argument> Arguments { get; set; }
    }

    public class Argument
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public ArgumentType Type { get; set; }
    }

    public enum ArgumentType
    {
        text = 0,
        number = 1,
        boolean = 2,
        datetime = 3,
        unknown = 4
    }
}
