# How to run kahla locally?

## Requirements

Kahla has some requirements for the system environment, in this document we are going to run in a Windows OS.

When you don't want to use Docker, the following requirements in need:

For Kahla server:

- .Net core >= 3.0
- [MSSQL](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

For Kahla web:

- Nodejs
- Angular
- ts-node
- uglify-js

## QuickStart

### __Step 1 : Get your app identity__

Create an app at [Aiursoft Developer Center](https://developer.aiursoft.com).

click "Go to Dashboard" -> "Create App", and fill in the relevant information and submit.

After create you will get a globally unique __AppID__ and __AppSecret__ to the local kahla app.

Then go to the _permission_ tag and enable the following permissions:

- View user's basic identity info
- View user's phone number
- Change user's phone number
- Change user's Email confirmation status
- Change user's basic info like nickname and bio
- Change the user's password based on source password

Go to the _OAuth settings_ mark "Enable OAuth"

Go to the _Site storge_ tag, create 2 site. (e.g. kahla-local-icon & kahla-local-files)

### __Step 2 : Setup Kahla server__

Create a new database named "KahlaLocal" in your mssql. Get the connection string.

Get Kahla server code:

```github cli
gh repo clone AiursoftWeb/Kahla
```

Change the settings of the kahla server ( ./Kahla.Server/appsettings.json ):

```json
  "ServerName": ,// Your app name in Step 1
  "KahlaAppId": ,// Your app id in Step 1
  "KahlaAppSecret": ,// Your app secret in Step 1
  "AutoAcceptRequests": false,
  "ConnectionStrings": {
    "DatabaseConnection": "Server=(localdb)\\mssqllocaldb;Database=KahlaLocal;Trusted_Connection=True;MultipleActiveResultSets=true", // Your connection string at the start of Step 2
    ...
    ...
  },
  "AppDomain": [
    ...
    ...
    {
      "Server": "localhost:your_port", // Your local Kahla server uri
      "Client": "http://localhost:your_angular_port"// Your local Kahla web uri (Will get it from the next step)
    }
  ],
  ...
  ...
  /** 
  * (Optional) Used for push notifications. 
  * Get it here: https://www.npmjs.com/package/web-push#generatevapidkeys
  */
  "VapidKeys": {
    "PublicKey": "<-public application server key->",
    "PrivateKey": "<-private application server key->"
  },
  ...
```

After setting, you can run the project

such as

```bash
dotnet run
```

Go _<http://localhost:your_port>_ you should see some thing like this

```json
{
  "wikiPath": "https://wiki.aiursoft.com",
  "serverTime": "2077-07-01T00:00:00.000000Z",
  "utcTime": "2077-07-01T00:00:00.000000Z",
  "apiVersion": "4.3.5",
  "vapidPublicKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "serverName": "TheNameOfTheServer",
  "mode": "Development",
  "domain": {
    "server": "http://localhost:your_port",
    "client": "http://localhost:your_angular_port"
  },
  "probe": {
    "playerFormat": "https://{0}.player.aiur.site",
    "endpoint": "https://probe.aiursoft.com",
    "openFormat": "https://{0}.aiur.site",
    "downloadFormat": "https://{0}.download.aiur.site"
  },
  "autoAcceptRequests": false,
  "code": 0,
  "message": "Welcome to Aiursoft Kahla API! Running in Development mode."
}

```

### __Step 3 : Setup Kahla web__

Get Kahla server code:

```github cli
gh repo clone AiursoftWeb/Kahla.App
```

Install packages in need

```powershell
npm install
```

( If you got error like this

```text
This version of npm is compatible with lockfileVersion@1, but package-lock.json was generated for lockfileVersion@2. I'll try to do my best with it!
```

 try remove ./package-lock.json )

(If still got error, try --force)

Pre-build

```powershell
 npm run prebuild
```

Run project

such as

```powershell
ng serve
```

After run project you can see the first page of the Kahla app on _<http://localhost:your_angular_port>_

Click "Server: Aiursoft Staging(or perhaps Server: xxxxxx)" under register button. Change the server address into _<http://localhost:your_port>_ in __Step 2__, click the ">CONNECT" button.

Click the the Sign in button and enjoy!
