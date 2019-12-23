using Aiursoft.Pylon;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kahla.SDK.Abstract
{
    public static class BotConfigurer
    {
        public static List<Type> AllAccessiableClass()
        {
            var entry = Assembly.GetEntryAssembly();
            return entry
                .GetReferencedAssemblies()
                .ToList()
                .Select(t => Assembly.Load(t))
                .AddWith(entry)
                .SelectMany(t => t.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsNestedPrivate)
                .Where(t => !t.IsGenericType)
                .Where(t => !t.IsInterface)
                .Where(t => !(t.Namespace?.StartsWith("System") ?? true))
                .ToList();
        }

        public static IServiceCollection AddBots(this IServiceCollection services)
        {
            var executingTypes = AllAccessiableClass()
                .Where(t => t.IsSubclassOf(typeof(BotBase)));
            foreach (var item in executingTypes)
            {
                services.AddScoped(typeof(BotBase), item);
            }
            return services;
        }
    }
}
