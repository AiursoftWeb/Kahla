# Aiursoft Kahla Backend

[![Build Status](https://travis-ci.org/AiursoftWeb/Kahla.svg?branch=master)](https://travis-ci.org/AiursoftWeb/Kahla)

Kahla is a cross-platform business messaging app. This is the server side code repo for Kahla.

## Requirements

Requirements about how to run
* [Windows Server](http://www.microsoft.com/en-us/cloud-platform/windows-server) or [Ubuntu Server](https://www.ubuntu.com/server)
* [.NET Core SDK 2.1.403 or later](https://github.com/dotnet/core/tree/master/release-notes)
* [Git](https://git-scm.com)
* [SQL Server](https://hub.docker.com/r/microsoft/mssql-server-linux/) (Strongly suggest installing it via docker)
* [VS Code](https://code.visualstudio.com) (Strongly suggest)

## How to run locally

1. Modify your `appsettings.json` to set all app settings to correct values.
    * Kahla is using SQL Server as this default database. Install SQL Server and set your connection string in `ConnectionString.DatabaseConnection`
    * Kahla is using Aiursoft integrated Authentication. Create a new app in [Aiursoft Developer Center](https://developer.aiursoft.com) and set your appId and appSecret
    * Kahla is using Aiursoft OSS to store files. Create a new bucket in [Aiursoft Developer Center](https://developer.aiursoft.com/buckets) and set your bucket id.
2. Execute `dotnet restore` under `./Kahla.Server` to restore all dotnet requirements
3. Execute `dotnet ef database update` to seed your database
4. Execute `dotnet run` to run the app
5. Use your browser to view [http://localhost:5000](http://localhost:5000)

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your personal workflow cruft out of sight.

We're also interested in your feedback for the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
