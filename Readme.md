# Kahla

<p align="center">
    <img width="100px" src="./kahla.png">
    <h3 align="center">Kahla</h3>
    <p align="center">Kahla is a cross-platform business messaging app.<p>
</p>

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/kahla/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/kahla/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/kahla/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/kahla/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/kahla/-/pipelines)
[![ManHours](https://manhours.aiursoft.cn/r/gitlab.aiursoft.cn/aiursoft/kahla.svg)](https://gitlab.aiursoft.cn/aiursoft/kahla/-/commits/master?ref_type=heads)
[![Website](https://img.shields.io/website?url=https%3A%2F%2Fkahla.aiursoft.cn%2F)](https://kahla.aiursoft.cn)
[![Docker](https://img.shields.io/badge/docker-latest-blue?logo=docker)](https://hub.aiursoft.cn/#!/taglist/aiursoft/kahla)

Kahla is a cross platform business chat application. It is written in C# and TypeScript. It uses ASP.NET Core and Angular. It is a part of the Aiursoft project.

This is the server project of Kahla.

## Try

Try a running Kahla [here](https://kahla.aiursoft.cn).

## Run manually

Before starting, you need to install MySQL.

```bash
sudo apt update
sudo apt install mysql-server
sudo mysql
```

Then:

```sql
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '<your_password>';
  exit;
```

Then:

```bash
sudo mysql -u root -p
```

Then:

```sql
CREATE DATABASE kahla;
CREATE USER 'kahla'@'localhost' IDENTIFIED BY '<kahla_password>';
GRANT ALL PRIVILEGES ON kahla.* TO 'kahla'@'localhost';
FLUSH PRIVILEGES;
exit;
````

Requirements about how to run

1. Install MySQL as instructions above.
2. Execute `dotnet run` to run the app.
3. Use your browser to view [http://localhost:5000](http://localhost:5000).

## Run in Microsoft Visual Studio

1. Open the `.sln` file in the project path.
2. Press `F5` to run the app.

## Run in Docker

First, install Docker [here](https://docs.docker.com/get-docker/).

Then run the following commands in a Linux shell:

```bash
image=hub.aiursoft.cn/aiursoft/kahla
appName=kahla
docker pull $image
docker run -d --name $appName --restart unless-stopped -p 5000:5000 -v /var/www/$appName:/data $image
```

That will start a web server at `http://localhost:5000` and you can test the app.

The docker image has the following context:

| Properties  | Value                           |
|-------------|---------------------------------|
| Image       | hub.aiursoft.cn/aiursoft/kahla  |
| Ports       | 5000                            |
| Binary path | /app                            |
| Data path   | /data                           |
| Config path | /data/appsettings.json          |

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you with push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
