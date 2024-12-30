# Kahla.SDK

Kahla.SDK is a library for writting bots and extends for Kahla.

[![NuGet version (Kahla.SDK)](https://img.shields.io/nuget/v/Kahla.SDK.svg?style=flat-square)](https://www.nuget.org/packages/Kahla.SDK/)
[![Build status](https://dev.azure.com/aiursoft/Star/_apis/build/status/Kahla%20Server%20Build)](https://dev.azure.com/aiursoft/Star/_build/latest?definitionId=6)

## Tutorial - How to create a bot with Kahla.SDK

This will introduce how to write a bot for Kahla. Before starting, make sure you have `.NET Core SDK` installed.

Download:

1. [.NET 9 SDK](http://dot.net/)

### 1. Create a new console .NET Core app

Open your terminal and type the following command to create a new console app.

```bash
mkdir MyBot
cd MyBot
dotnet new console
```

### 2. Add dependency for Kahla.SDK

Execute the following command to add Kahla.SDK as a dependency.

```bash
dotnet add package Kahla.SDK
```

### 3. Create your bot

Create a new file, and name it `FirstBot.cs`. In this C# class, extend the class `BotBase`. Override the default `OnMessage` method.

```csharp
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using System.Threading.Tasks;

namespace MyBot
{
    public class FirstBot : BotBase
    {
        public async override Task OnMessage(string inputMessage, NewMessageEvent eventContext) 
        {
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return; // Ignore messages sent by itself.
            }
            // Echo all messages.
            await SendMessage(inputMessage, eventContext.ConversationId);
        }
    }
}
```

### 4. Create your bot start up logic

Modify your `Program.cs` to start your bot.

```csharp
using Kahla.SDK.Abstract;
using System.Threading.Tasks;

namespace MyBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await new BotBuilder()
                .Build<FirstBot>()
                .Run();
        }
    }
}
```

### 5. Start your bot

Execute the following command to start your bot.

```bash
dotnet run
```

![demo](https://github.com/AiursoftWeb/Kahla/raw/dev/Kahla.SDK/Pics/rundemo.png)


You need to sign in an Aiursoft account which identies your bot. If you don't have one, register [here](https://server.kahla.app/Auth/GoRegister).

After your bot is started, just talk to it with another account!

![demo](https://github.com/AiursoftWeb/Kahla/raw/dev/Kahla.SDK/Pics/botchatdemo.png)

That's all! Happy coding!

### 6. Additional info

For dependency injection and advanced start up, Kahla.Bot supports custom start up configure.

Modify your `Program.cs` like this to use advanced start up:

```csharp
using Kahla.Bot.Bots;
using Kahla.SDK.Abstract;
using System.Linq;
using System.Threading.Tasks;

namespace MyBot
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateBotBuilder()
                .Build<FirstBot>()
                .Run();
        }

        public static BotBuilder CreateBotBuilder()
        {
            return new BotBuilder()
                .UseStartUp<StartUp>();
        }
    }
}
```

And create a new class named: `StartUp`:

```csharp
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyBot
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add your own services.
            services.AddTransient<YourTransientService>();
            services.AddScoped<YourScopedService>();
            services.AddSingleton<YourSingletonService>();
        }

        public void Configure()
        {
            // This will execute after services are configured. You can edit some global settings here.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }
    }
}
```

For more bot demo, please search `bot.kahla.app` in Kahla. Or [view more demos](https://github.com/AiursoftWeb/Kahla/tree/dev/Kahla.Bot);
