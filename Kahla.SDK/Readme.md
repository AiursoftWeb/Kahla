# Kahla.SDK

Kahla.SDK is a library for writting bots and extends for Kahla.

[![NuGet version (Kahla.SDK)](https://img.shields.io/nuget/v/Kahla.SDK.svg?style=flat-square)](https://www.nuget.org/packages/Kahla.SDK/)
[![Build status](https://dev.azure.com/aiursoft/Star/_apis/build/status/Kahla%20Server%20Build)](https://dev.azure.com/aiursoft/Star/_build/latest?definitionId=6)

## Tutorial - How to create a bot with Kahla.SDK

This will introduce how to write a bot for Kahla. Before starting, make sure you have `.NET Core SDK` installed.

Download .NET Core SDK [here](http://dot.net).

### 1. Create a new console .NET Core app

Open your terminal and type the following command to create a new console app.

```bash
$ mkdir MyBot
$ cd MyBot
$ dotnet new console
```

### 2. Add dependency for Kahla.SDK

Execute the following command to add Kahla.SDK as a dependency.

```bash
$ dotnet add package Kahla.SDK
```

### 3. Create your bot

Create a new file, and name it `FirstBot.cs`. In this C# class, extend the class `BotBase`. Implement all methods in it.

```csharp
using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;
using System.Threading.Tasks;

namespace MyBot
{
    public class FirstBot : BotBase
    {
        public async override Task OnBotInit() { }

        public async override Task OnFriendRequest(NewFriendRequestEvent arg) { }

        public async override Task OnGroupConnected(SearchedGroup group) { }

        public async override Task OnGroupInvitation(int groupId, NewMessageEvent eventContext) { }

        public async override Task OnMessage(string inputMessage, NewMessageEvent eventContext) 
        {
            if (eventContext.Message.SenderId == Profile.Id)
            {
                return;
            }
            // Echo all messages.
            await SendMessage(inputMessage, eventContext.ConversationId, eventContext.AESKey);
        }
    }
}
```

### 4. Create your bot start up logic

Modify your `Program.cs` to start your bot.

```csharp
using Aiursoft.Scanner;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace MyBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await new ServiceCollection()
                // Add all dependencies.
                .AddScannedDependencies()
                // Register your bot.
                .AddBots()
                // Get your bot.
                .GetService<FirstBot>()
                // Start your bot.
                .Start();
        }
    }
}

```

### 5. Start your bot

Execute the following command to start your bot.

```bash
$ dotnet run
```

![demo](https://github.com/AiursoftWeb/Kahla/raw/dev/Kahla.SDK/Pics/rundemo.png)


You need to sign in an Aiursoft account which identies your bot. If you don't have one, register [here](https://server.kahla.app/Auth/GoRegister).

After your bot is started, just talk to it with another account!

![demo](https://github.com/AiursoftWeb/Kahla/raw/dev/Kahla.SDK/Pics/botchatdemo.png)

That's all! Happy coding!

### 6. Additional info

For more bot demo, please search `bot.kahla.app` in Kahla. Or [view more demos](https://github.com/AiursoftWeb/Kahla/tree/dev/Kahla.Bot/Bots);
