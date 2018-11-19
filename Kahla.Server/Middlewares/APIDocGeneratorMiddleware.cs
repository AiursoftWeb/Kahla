using Aiursoft.Pylon.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kahla.Server.Middlewares
{
    public class APIDocGeneratorMiddleware
    {
        private IConfiguration _configuration { get; }
        private RequestDelegate _next;

        public APIDocGeneratorMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _configuration = configuration;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().ToLower() != "/doc")
            {
                await _next.Invoke(context);
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            var actionsMatches = new List<API>();
            foreach (var controller in Assembly.GetEntryAssembly().GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)))
            {
                if (!IsController(controller))
                {
                    continue;
                }
                foreach (var method in controller.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                {
                    if (!IsAction(method))
                    {
                        continue;
                    }
                    var args = GenerateArguments(method);
                    var api = new API
                    {
                        ControllerName = controller.Name,
                        ActionName = method.Name,
                        IsPost = method.CustomAttributes.Any(t => t.AttributeType == typeof(HttpPostAttribute)),
                        Arguments = args,
                        AuthRequired = JudgeAuthorized(method, controller)
                    };
                    actionsMatches.Add(api);
                }
            }
            await context.Response.WriteAsync(JsonConvert.SerializeObject(actionsMatches));
            return;
        }

        private List<Argument> GenerateArguments(MethodInfo method)
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
            return args;
        }

        private bool IsController(Type type)
        {
            return
                type.Name.EndsWith("Controller") &&
                type.Name != "Controller" &&
                type.IsSubclassOf(typeof(Controller)) &&
                type.IsPublic;
        }

        private bool IsAction(MethodInfo method)
        {
            return
                !method.IsAbstract &&
                !method.IsVirtual &&
                !method.IsStatic &&
                !method.IsConstructor &&
                !method.IsDefined(typeof(NonActionAttribute));
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

        private bool JudgeAuthorized(MethodInfo action, Type controller)
        {
            return
                action.CustomAttributes.Any(t => t.AttributeType == typeof(AiurForceAuth)) ||
                controller.CustomAttributes.Any(t => t.AttributeType == typeof(AiurForceAuth));
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
}
