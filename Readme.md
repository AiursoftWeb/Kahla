# Aiursoft Kahla Backend

[![Build status](https://aiursoft.visualstudio.com/Star/_apis/build/status/Kahla%20Server%20Build)](https://aiursoft.visualstudio.com/Star/_build/latest?definitionId=6)
[![NuGet version (Kahla.SDK)](https://img.shields.io/nuget/v/Kahla.SDK.svg?style=flat-square)](https://www.nuget.org/packages/Kahla.SDK/)
[![Issues](https://img.shields.io/github/issues/AiursoftWeb/Kahla.svg)](https://github.com/AiursoftWeb/Kahla/issues)

Kahla is a cross-platform business messaging app. This is the server side code repo for Kahla.

If you are writting bots and extends for Kahla, view SDK document [here](./Kahla.SDK/Readme.md).

## Requirements

Requirements about how to run:

* [.NET Core 3.0 runtime](https://dotnet.microsoft.com/download/dotnet-core/3.0)
* [SQL Server](https://hub.docker.com/r/microsoft/mssql-server-linux/)

## How to run locally

1. Modify your `appsettings.json` to set all app settings to correct values.
    * Kahla is using SQL Server as this default database. Install SQL Server and set your connection string in `ConnectionString.DatabaseConnection`
    * Kahla is using Aiursoft integrated Authentication. Create a new app in [Aiursoft Developer Center](https://developer.aiursoft.com) and set your appId and appSecret
    * Make sure you enabled `OAuth` and `Debug mode` for you app.
    * Kahla is using Aiursoft Probe to store files. Create a new site in [Aiursoft Developer Center](https://developer.aiursoft.com/) and set your site name.
2. Execute `dotnet run` to run the app
3. Use your browser to view [http://localhost:5000](http://localhost:5000)

## Run in Ubuntu 18.04

First, specify a domain name for your Ubuntu 18.04, brand new server.

And execute the following command in the server:

```bash
$ curl -sL https://github.com/AiursoftWeb/Kahla/raw/dev/install.sh | sudo bash -s www.example.com
```

To uninstall:

```bash
$ curl -sL https://github.com/AiursoftWeb/Kahla/raw/dev/uninstall.sh | sudo bash
```

## How to publish Kahla server to production environment

This guide explains setting up a production-ready Kahla environment on an Ubuntu 16.04 server. These instructions likely work with newer versions of Ubuntu, but the instructions haven't been tested with newer versions.

For information on other Linux distributions supported by ASP.NET Core, see [Prerequisites for .NET Core on Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites).

> For Ubuntu 14.04, *supervisord* is recommended as a solution for monitoring the Kestrel process. *systemd* isn't available on Ubuntu 14.04. For Ubuntu 14.04 instructions, see the [previous version of this topic](https://github.com/aspnet/AspNetCore.Docs/blob/e9c1419175c4dd7e152df3746ba1df5935aaafd5/aspnetcore/publishing/linuxproduction.md).

This guide:

* Places Kahla behind a reverse proxy server. But only the server side app of Kahla. If you consider deploying the client side of Kahla, please see [here](https://github.com/AiursoftWeb/Kahla.App).
* Sets up the reverse proxy server to forward requests to the Kestrel web server.
* Ensures the web app runs on startup as a daemon.
* Configures a process management tool to help restart the web app.

### Prerequisites

1. Access to an Ubuntu 16.04 server with a standard user account with sudo privilege.
2. Install the .NET Core runtime on the server.
   1. Visit the [.NET Core All Downloads page](https://www.microsoft.com/net/download/all).
   1. Select the latest non-preview runtime from the list under **Runtime**.
   1. Select and follow the instructions for Ubuntu that match the Ubuntu version of the server.
3. Kahla source code (Only server side).
4. Docker on Linux

### Install SQL Server for docker on Ubuntu

We suggest you installing SQL Server for docker. Please read:

https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15

Or simpliy execute:

```bash
docker run --name sqlenterprise \
      -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>' \
      -e 'MSSQL_PID=Enterprise' -p 1433:1433 \
      -d mcr.microsoft.com/mssql/server:2017-latest
```

If you want to install the SQL Server on the raw Ubuntu server, please read:

https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15

> The default command of installing SQL Server is installing its developer edition. Please add argument: 'MSSQL_PID=Enterprise' to use the enterprise edition of SQL Server.

## Prepare the settings for Kahla

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

### Publish and copy over the app

Run [dotnet publish](/dotnet/core/tools/dotnet-publish) from the development environment to package an app into a directory (for example, *bin/Release/&lt;target_framework_moniker&gt;/publish*) that can run on the server:

```dotnetcli
dotnet publish -c Release
```

Copy the ASP.NET Core app to the server using a tool that integrates into the organization's workflow (for example, SCP, SFTP). It's common to locate web apps under the *var* directory (for example, *var/www/kahla*).

> Under a production deployment scenario, a continuous integration workflow does the work of publishing the app and copying the assets to the server.

Test the app:

1. From the command line, run the app: `dotnet <app_assembly>.dll`.
1. In a browser, navigate to `http://<serveraddress>:<port>` to verify the app works on Linux locally.

### Configure a reverse proxy server

A reverse proxy is a common setup for serving dynamic web apps. A reverse proxy terminates the HTTP request and forwards it to the ASP.NET Core app.

#### Use a reverse proxy server

Kestrel is great for serving dynamic content from ASP.NET Core. However, the web serving capabilities aren't as feature rich as servers such as IIS, Apache, or Nginx. A reverse proxy server can offload work such as serving static content, caching requests, compressing requests, and HTTPS termination from the HTTP server. A reverse proxy server may reside on a dedicated machine or may be deployed alongside an HTTP server.

For the purposes of this guide, a single instance of Nginx is used. It runs on the same server, alongside the HTTP server. Based on requirements, a different setup may be chosen.

#### Install Nginx

Use `apt-get` to install Nginx. The installer creates a *systemd* init script that runs Nginx as daemon on system startup. Follow the installation instructions for Ubuntu at [Nginx: Official Debian/Ubuntu packages](https://www.nginx.com/resources/wiki/start/topics/tutorials/install/#official-debian-ubuntu-packages).

> If optional Nginx modules are required, building Nginx from source might be required.

Since Nginx was installed for the first time, explicitly start it by running:

```bash
sudo service nginx start
```

Verify a browser displays the default landing page for Nginx. The landing page is reachable at `http://<server_IP_address>/index.nginx-debian.html`.

#### Configure Nginx

To configure Nginx as a reverse proxy to forward requests to your ASP.NET Core app, modify */etc/nginx/sites-available/default*. Open it in a text editor, and replace the contents with the following:

```nginx
server {
    listen        80;
    server_name   example.com *.example.com;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   x-request-origin Aiursoft;
        proxy_cache_bypass $http_upgrade;
    }
}
```

When no `server_name` matches, Nginx uses the default server. If no default server is defined, the first server in the configuration file is the default server. As a best practice, add a specific default server which returns a status code of 444 in your configuration file. A default server configuration example is:

```nginx
server {
    listen   80 default_server;
    # listen [::]:80 default_server deferred;
    return   444;
}
```

With the preceding configuration file and default server, Nginx accepts public traffic on port 80 with host header `example.com` or `*.example.com`. Requests not matching these hosts won't get forwarded to Kestrel. Nginx forwards the matching requests to Kestrel at `http://localhost:5000`. See [How nginx processes a request](https://nginx.org/docs/http/request_processing.html) for more information. To change Kestrel's IP/port, see [Kestrel: Endpoint configuration](xref:fundamentals/servers/kestrel#endpoint-configuration).

> Failure to specify a proper [server_name directive](https://nginx.org/docs/http/server_names.html) exposes your app to security vulnerabilities. Subdomain wildcard binding (for example, `*.example.com`) doesn't pose this security risk if you control the entire parent domain (as opposed to `*.com`, which is vulnerable). See [rfc7230 section-5.4](https://tools.ietf.org/html/rfc7230#section-5.4) for more information.

Once the Nginx configuration is established, run `sudo nginx -t` to verify the syntax of the configuration files. If the configuration file test is successful, force Nginx to pick up the changes by running `sudo nginx -s reload`.

To directly run the app on the server:

1. Navigate to the app's directory.
1. Run the app: `dotnet <app_assembly.dll>`, where `app_assembly.dll` is the assembly file name of the app.

If the app runs on the server but fails to respond over the Internet, check the server's firewall and confirm that port 80 is open. If using an Azure Ubuntu VM, add a Network Security Group (NSG) rule that enables inbound port 80 traffic. There's no need to enable an outbound port 80 rule, as the outbound traffic is automatically granted when the inbound rule is enabled.

When done testing the app, shut the app down with `Ctrl+C` at the command prompt.

### Monitor Kahla

The server is setup to forward requests made to `http://<serveraddress>:80` on to the ASP.NET Core app running on Kestrel at `http://127.0.0.1:5000`. However, Nginx isn't set up to manage the Kestrel process. *systemd* can be used to create a service file to start and monitor the underlying web app. *systemd* is an init system that provides many powerful features for starting, stopping, and managing processes. 

#### Create Kahla service file

Create the service definition file:

```bash
sudo vim /etc/systemd/system/kestrel-Kahla.service
```

The following is an example service file for the app:

```ini
[Unit]
Description=Kahla App running on Ubuntu

[Service]
WorkingDirectory=/var/www/kahla
ExecStart=/usr/bin/dotnet /var/www/kahla/Kahla.Server.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

If the user *www-data* isn't used by the configuration, the user defined here must be created first and given proper ownership for files.

Use `TimeoutStopSec` to configure the duration of time to wait for the app to shut down after it receives the initial interrupt signal. If the app doesn't shut down in this period, SIGKILL is issued to terminate the app. Provide the value as unitless seconds (for example, `150`), a time span value (for example, `2min 30s`), or `infinity` to disable the timeout. `TimeoutStopSec` defaults to the value of `DefaultTimeoutStopSec` in the manager configuration file (*systemd-system.conf*, *system.conf.d*, *systemd-user.conf*, *user.conf.d*). The default timeout for most distributions is 90 seconds.

```
# The default value is 90 seconds for most distributions.
TimeoutStopSec=90
```

Linux has a case-sensitive file system. Setting ASPNETCORE_ENVIRONMENT to "Production" results in searching for the configuration file *appsettings.Production.json*, not *appsettings.production.json*.

Some values (for example, SQL connection strings) must be escaped for the configuration providers to read the environment variables. Use the following command to generate a properly escaped value for use in the configuration file:

```console
systemd-escape "<value-to-escape>"
```

Colon (`:`) separators aren't supported in environment variable names. Use a double underscore (`__`) in place of a colon. The [Environment Variables configuration provider](xref:fundamentals/configuration/index#environment-variables-configuration-provider) converts double-underscores into colons when environment variables are read into configuration. In the following example, the connection string key `ConnectionStrings:DefaultConnection` is set into the service definition file as `ConnectionStrings__DefaultConnection`:

```
Environment=ConnectionStrings__DefaultConnection={Connection String}
```

Save the file and enable the service.

```bash
sudo systemctl enable kestrel-kahla.service
```

Start the service and verify that it's running.

```bash
sudo systemctl start kestrel-kahla.service
sudo systemctl status kestrel-kahla.service

● kestrel-kahla.service - Example .NET Web API App running on Ubuntu
    Loaded: loaded (/etc/systemd/system/kestrel-kahla.service; enabled)
    Active: active (running) since Thu 2016-10-18 04:09:35 NZDT; 35s ago
Main PID: 9021 (dotnet)
    CGroup: /system.slice/kestrel-helloapp.service
            └─9021 /usr/local/bin/dotnet /var/www/helloapp/helloapp.dll
```

With the reverse proxy configured and Kestrel managed through systemd, the web app is fully configured and can be accessed from a browser on the local machine at `http://localhost`. It's also accessible from a remote machine, barring any firewall that might be blocking. Inspecting the response headers, the `Server` header shows the ASP.NET Core app being served by Kestrel.

```text
HTTP/1.1 200 OK
Date: Tue, 11 Oct 2016 16:22:23 GMT
Server: Kestrel
Keep-Alive: timeout=5, max=98
Connection: Keep-Alive
Transfer-Encoding: chunked
```

#### View logs

Since the web app using Kestrel is managed using `systemd`, all events and processes are logged to a centralized journal. However, this journal includes all entries for all services and processes managed by `systemd`. To view the `kestrel-kahla.service`-specific items, use the following command:

```bash
sudo journalctl -fu kestrel-kahla.service
```

For further filtering, time options such as `--since today`, `--until 1 hour ago` or a combination of these can reduce the amount of entries returned.

```bash
sudo journalctl -fu kestrel-kahla.service --since "2016-10-18" --until "2016-10-18 04:00"
```

### Secure the app

#### Enable AppArmor

Linux Security Modules (LSM) is a framework that's part of the Linux kernel since Linux 2.6. LSM supports different implementations of security modules. [AppArmor](https://wiki.ubuntu.com/AppArmor) is a LSM that implements a Mandatory Access Control system which allows confining the program to a limited set of resources. Ensure AppArmor is enabled and properly configured.

#### Configure the firewall

Close off all external ports that are not in use. Uncomplicated firewall (ufw) provides a front end for `iptables` by providing a command line interface for configuring the firewall.

> A firewall will prevent access to the whole system if not configured correctly. Failure to specify the correct SSH port will effectively lock you out of the system if you are using SSH to connect to it. The default port is 22. For more information, see the [introduction to ufw](https://help.ubuntu.com/community/UFW) and the [manual](https://manpages.ubuntu.com/manpages/bionic/man8/ufw.8.html).

Install `ufw` and configure it to allow traffic on any ports needed.

```bash
sudo apt-get install ufw

sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

sudo ufw enable
```

#### Secure Nginx

#### Change the Nginx response name

Edit *src/http/ngx_http_header_filter_module.c*:

```
static char ngx_http_server_string[] = "Server: Web Server" CRLF;
static char ngx_http_server_full_string[] = "Server: Web Server" CRLF;
```

#### Configure options

Configure the server with additional required modules. Consider using a web app firewall, such as [ModSecurity](https://www.modsecurity.org/), to harden the app.

#### HTTPS configuration

**Configure the app for secure (HTTPS) local connections**

The [dotnet run](/dotnet/core/tools/dotnet-run) command uses the app's *Properties/launchSettings.json* file, which configures the app to listen on the URLs provided by the `applicationUrl` property (for example, `https://localhost:5001;http://localhost:5000`).

Configure the app to use a certificate in development for the `dotnet run` command or development environment (F5 or Ctrl+F5 in Visual Studio Code) using one of the following approaches:

* [Replace the default certificate from configuration](xref:fundamentals/servers/kestrel#configuration) (*Recommended*)
* [KestrelServerOptions.ConfigureHttpsDefaults](xref:fundamentals/servers/kestrel#configurehttpsdefaultsactionhttpsconnectionadapteroptions)

**Configure the reverse proxy for secure (HTTPS) client connections**

* Configure the server to listen to HTTPS traffic on port `443` by specifying a valid certificate issued by a trusted Certificate Authority (CA).

* Harden the security by employing some of the practices depicted in the following */etc/nginx/nginx.conf* file. Examples include choosing a stronger cipher and redirecting all traffic over HTTP to HTTPS.

Please check official nginx document: https://nginx.org/en/docs/http/configuring_https_servers.html

#### Secure Nginx from clickjacking

[Clickjacking](https://blog.qualys.com/securitylabs/2015/10/20/clickjacking-a-common-implementation-mistake-that-can-put-your-websites-in-danger), also known as a *UI redress attack*, is a malicious attack where a website visitor is tricked into clicking a link or button on a different page than they're currently visiting. Use `X-FRAME-OPTIONS` to secure the site.

To mitigate clickjacking attacks:

1. Edit the *nginx.conf* file:

   ```bash
   sudo nano /etc/nginx/nginx.conf
   ```

   Add the line `add_header X-Frame-Options "SAMEORIGIN";`.
1. Save the file.
1. Restart Nginx.

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your personal workflow cruft out of sight.

We're also interested in your feedback for the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
