# Aiursoft Kahla Backend

Kahla is a cross-platform business messaging app. This is the server side code repo for Kahla.

If you are writting bots and extends for Kahla, view SDK document [here](./Kahla.SDK/Readme.md).

## How to install

### Brief steps:

* Get a domain name. (Like kahla.example.com)
* Get a server.
* Create an app at [Aiursoft Developer Center](https://developer.aiursoft.com).
* Config your app.
* Install on your server.

### Get a server

Get a brand new Ubuntu 18.04 server.

  * Server must be Ubuntu 18.04. (20.04 and 16.04 is not supported)
  * Server must have a public IP address. (No local VM)
  * Server must have access to the global Internet. (Not Chinese network)

Vultr or DigitalOcean is suggested. [Get it from Vultr](https://www.vultr.com/?ref=7274488).

### Create an app

Go to [Aiursoft Developer Center](https://developer.aiursoft.com). Sign in your account. And then create an app.

Enable the following permissions:

* View user's basic identity info
* View user's phone number
* Change user's phone number
* Change user's Email confirmation status
* Change user's basic info like nickname and bio
* Change the user's password based on source password

Enable OAuth. Set your domain name.

Grab your `appId` and `appSecret`.

### Install on server

Execute the following command on the server:

```bash
$ curl -sL https://github.com/AiursoftWeb/Kahla/raw/master/install.sh | sudo bash -s kahla.example.com yourappid yourappsecret
```

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your personal workflow cruft out of sight.

We're also interested in your feedback for the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
