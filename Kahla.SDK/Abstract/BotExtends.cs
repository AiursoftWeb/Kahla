using Kahla.SDK.Data;
using Kahla.SDK.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kahla.SDK.Abstract
{
    public static class BotExtends
    {
        private static IEnumerable<Type> ScanBots()
        {
            var bots = Assembly
                .GetEntryAssembly()
                ?.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BotBase)));
            return bots;
        }

        private static IEnumerable<Type> ScanHandler()
        {
            var handlers = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => i.Name.StartsWith(nameof(ICommandHandler<BotBase>))));
            return handlers;
        }

        public static IServiceCollection AddBots(this IServiceCollection services)
        {
            // Register the bots.
            foreach (var botType in ScanBots())
            {
                services.AddScoped(botType);
                foreach (var handler in ScanHandler())
                {
                    services.AddScoped(typeof(ICommandHandler<>).MakeGenericType(botType), handler.MakeGenericType(botType));
                }
            }

            services.AddSingleton(typeof(EventSyncer<>));
            services.AddScoped(typeof(BotHost<>));
            services.AddScoped(typeof(BotCommander<>));
            services.AddScoped(typeof(BotFactory<>));
            services.AddScoped(typeof(CommandFactory<>));
            return services;
        }
    }
}
