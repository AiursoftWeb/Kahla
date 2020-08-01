# Aiursoft Kahla Backend

[![Build status](https://aiursoft.visualstudio.com/Star/_apis/build/status/Kahla%20Server%20Build)](https://aiursoft.visualstudio.com/Star/_build/latest?definitionId=6)
[![NuGet version (Kahla.SDK)](https://img.shields.io/nuget/v/Kahla.SDK.svg?style=flat-square)](https://www.nuget.org/packages/Kahla.SDK/)
[![Issues](https://img.shields.io/github/issues/AiursoftWeb/Kahla.svg)](https://github.com/AiursoftWeb/Kahla/issues)

Kahla is a cross-platform business messaging app. This is the server side code repo for Kahla.

If you are writting bots and extends for Kahla, view SDK document [here](./Kahla.SDK/Readme.md).

## How to deploy

### Brief steps:

* Get a domain name. (Like kahla.example.com)
* Get a brand new Ubuntu 18.04 server.
  * Server must have a public IP address. (No local VM)
  * Server must have access to the global Internet. (Not Chinese network)
  * Vultr or DigitalOcean is suggested. [Get it from Vultr](https://www.vultr.com/?ref=7274488)
* Create a new app at [Aiursoft Developer Center](https://developer.aiursoft.com).
* Point your domian name to your server's IP.
* Connect to your server via `ssh`.

Grab your `appId` and `appSecret` in [Aiursoft Developer Center](https://developer.aiursoft.com). Copy your domian name.

And execute the following command in the server:

```bash
$ curl -sL https://github.com/AiursoftWeb/Kahla/raw/master/install.sh | sudo bash -s kahla.example.com yourappid yourappsecret
```

To uninstall:

```bash
$ curl -sL https://github.com/AiursoftWeb/Kahla/raw/master/uninstall.sh | sudo bash
```

## How to run locally

### Requirements

Requirements about how to run:

* [.NET Core 3.0 runtime](https://dotnet.microsoft.com/download/dotnet-core/3.0)
* [SQL Server](https://hub.docker.com/r/microsoft/mssql-server-linux/)


### Prepare the settings for Kahla

The default settings of Kahla is:

```javascript
{
   // Used for checking updates. If you have forked Kahla.CLI, change it to your own repo.
  "CLIMasterPackageJson": "https://raw.githubusercontent.com/AiursoftWeb/Kahla.CLI/master/package.json",

   // Used for checking updates. If you have forked Kahla.App, change it to your own repo.
  "KahlaMasterPackageJson": "https://raw.githubusercontent.com/AiursoftWeb/Kahla.App/master/package.json",

   // Used for cross-domain cookie settings. Change the `Server` to your production server domian, and change the `Client` to your production app domain.
  "AppDomain": [
    {
      // The domain name which server serves requests.
      "Server": "server.kahla.app", 
      // In this server domian, which domian allows cookie.
      "Client": "https://web.kahla.app" 
    }
  ],

  // Used for database connection. Change it to your local SQL Server database.
  "ConnectionStrings": {
    "DatabaseConnection": "Server=(localdb)\\mssqllocaldb;Database=KahlaLocal;Trusted_Connection=True;MultipleActiveResultSets=true"
  },

  // Used for email notification settings.
  "EmailAppDomain": "https://web.kahla.app",
  "MailUser": "service@aiursoft.com",
  "MailPassword": "YourStrongPassword",
  "MailServer": "box.aiursoft.com",

  // Used for integrated authentication and site storage. Get it on https://developer.aiursoft.com.
  "KahlaAppId": "<-Your app Id->",
  "KahlaAppSecret": "<-Your app secret->",

  // Site for storage users' icons.
  "UserIconsSiteName": "kahla-user-icon",
  // Site for storage users' files.
  "UserFilesSiteName": "kahla-user-files",
  // Default group icon.
  "GroupImagePath": "kahla-user-icon/default.png",

  // Logging settings.
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },

  // Used for push notifications. Get it here: https://www.npmjs.com/package/web-push#generatevapidkeys
  "VapidKeys": {
    "PublicKey": "<-public application server key->",
    "PrivateKey": "<-private application server key->"
  }
}
```

Modify your `appsettings.json` to set all app settings to correct values.

* Kahla is using SQL Server as this default database. Install SQL Server and set your connection string in `ConnectionString:DatabaseConnection`
* Kahla is using Aiursoft integrated Authentication. Create a new app in [Aiursoft Developer Center](https://developer.aiursoft.com) and set your appId and appSecret
* Make sure you enabled `OAuth` for you app. Set the `App Domain` settings in the developer center to your **Kahla server domain after reverse proxy** not your Kahla.App domian. This is to make sure your server can successfully pass the OAuth settings.
* Kahla is using Aiursoft Probe to store files. Create two new sites in [Aiursoft Developer Center](https://developer.aiursoft.com/) and set your site name in the appsettings.json.
* Set your Email server settings.
* Set your vapid keys. Get it from: https://www.npmjs.com/package/web-push#generatevapidkeys
* Set your app domain. Kahla will detect the requesting url by your `Server` value and return a `access-control-allow-origin` header to the client to help passing cookie. In this example, all requests comes from the nginx reverse-proxy server. In this case you need to set it to `localhost`.

Execute `dotnet run` to run the app

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your personal workflow cruft out of sight.

We're also interested in your feedback for the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
