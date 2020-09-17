# Kahla API Document

## Branchs

There are two branch of Kahla. One is master (also called 'Production'). The other one is dev (also called 'Staging').

```
Kahla's Servers
│
├───Production (Production DB)
│   ├───Kahla App
│   │   └───web.kahla.app
│   ├───Kahla Home
│   │   └───www.kahla.app
│   └───Kahla Server
│       └───server.kahla.app
│
└───Staging (Staging DB)
    ├───Kahla App
    │   └───staging.web.kahla.app
    ├───Kahla Home
    │   └───staging.kahla.app
    └───Kahla Server
        ├───dev.server.kahla.app
        └───staging.server.kahla.app
```

| Server Address | Allow Cross-Domain | Code Branch | Usage | Corresponding Kahla Client |
|-|-|-|-|-|
| server.kahla.app | [https://web.kahla.app](https://web.kahla.app) | master | hosting production version of Kahla server | master |
| staging.server.kahla.app | [https://staging.web.kahla.app](https://staging.web.kahla.app) | dev | hosting staging version of the Kahla server | dev |
| dev.server.kahla.app | [http://localhost:4200](http://localhost:4200) | dev | For developers locally debugging Kahla.App | Local |

There are two servers for Kahla home page.

| Server Address | App domain | Code Branch | Usage | Corresponding Kahla Client |
|-|-|-|-|-|
| www.kahla.app | [https://web.kahla.app](https://web.kahla.app) | master | hosting production version of Kahla server | master |
| staging.kahla.app | [https://staging.web.kahla.app](https://staging.web.kahla.app) | dev | hosting staging version of the Kahla server | dev |

## Protocol

All of Kahla's server communications requires HTTPS. And support HSTS.

## Database

The database of the staging server and the local debugging server is the same one, and the database may be cleaned regularly, and the data persistence is not guaranteed.

The production Kahla server uses an independent database, and the database will never be cleaned up.

If you are a contributor to Kahla, please note that the debugging API is used in the debugging environment, the staging API is used in the pre-deployment environment, and the production API is used only in the production environment.

## Authentication

Kahla server uses cookie-based authentication. In other words, all cookies must be carried when accessing all APIs that require login privileges.

When Kahla's client is running, it must work in the cross-domain address allowed by the target server, otherwise it will not be able to carry cookies.

For available domains, please reference the serer address.

## Version checking

We have two meaning of word `Version`.

* API Level
* App version

API level is the communication protocol level. The version of the Kahla API. Can only get it from server `/`. For example, to check the API level of Kahla server: `https://staging.server.kahla.app`, just access: https://staging.server.kahla.app and the version will be shown.

The App version is the version of the current Kahla app. Ideally, it is the same as the API level to which server you are connecting to. And it is the same as the latest Kahla.App version.

To check the latest app version, access: https://staging.kahla.app/version (For production, it is: https://www.kahla.app/version)

The logic for apps:

* If the current app version is not latest with `https://staging.kahla.app/version`, show a warning.
* If the current app version is not the same with the API level on the server, show a warning.

The app needs to hard code only two address:

* https://staging.kahla.app/version (Check app version)
* https://staging.kahla.app/servers (Check server list)

For some other users may deploy their own version of Kahla. If all `Kahla.Server` can check the version of the current app, which means that the response may be fake. And `Kahla.Server` maintainers shall not be concerned with the app versions. They only need to care about their API level. So we need only one version checking endpoint no matter which server the app is connecting to.

## File uploading

This API connects to our new storage base: Probe. Probe is powerful. Supports multiple sites and folders.

The document of the API is [here](https://wiki.aiursoft.com/Kahla/Storage.md)

Different conversations have different folders. And different users have different permissions to access. Which makes files safer to save, and API simpler.

Probe API returns file path with a pattern: `sitename/path/filename`.

For example, if you call the [Init icon upload API](https://wiki.aiursoft.com/Kahla/Storage.md#InitIconUpload), the server will return you an upload path like:

```json
{
  "value":"https://probe.aiursoft.com/Files/UploadFile/kahla-user-icon/2019-11-11?pbtoken=eyJzaXRlTmFtZSI6ImthaGxhLXVzZXItaWNvbiIsInVuZGVyUGF0aCI6IjIwMTktMTEtMTEiLCJwZXJtaXNzaW9ucyI6IlVwbG9hZCIsImV4cGlyZXMiOiIyMDE5LTExLTExVDA0OjE1OjI2LjEyMDc3MVoifQ%3D%3D.G%2BEA4PWzfmLPWpKeSUbaufqzs7WyncHdXZPTj4Ca8jTcoIwX%2F6kskbBqXJNjJmhVW3j%2BtpMemxBaDl6pu0XnwvKXrJHip21yM8Jp6eTX4Vd9cSvLS2feD44VuybQ6liQlcWhXk%2F%2FWyUkESZilIQRFp2Gs9OJ0lObGBc5MLnDs8s%3D&recursivecreate=True",
  "code":0,
  "message":"Token is given. You can not upload your file to that address. And your will get your response as 'FilePath'."}
```

When you get the upload path, you can upload a file to it. The document of the upload API is [here](https://wiki.aiursoft.com/Integrated%20Website/Files.md#UploadFile).

When you successfully uploaded a file, possible response is:

```json
{
  "siteName": "kahla-user-icon",
  "filePath": "kahla-user-icon/2019-11-11/IMG_20190924_193959.jpg",
  "internetPath": "https://kahla-user-icon.aiursoft.io/2019-11-11/IMG_20190924_193959.jpg",
  "fileSize": 1718140,
  "code": 0,
  "message": "Successfully uploaded your file."
}
```

We use the `filePath` with `sitename/path/filename` pattern to represent a file instead of a file ID. You should use this pattern to save and send messages representing files instead of the file id.

> filePath is not url encoded. And it can not be used as download address directly.

**Never use the `internetPath` property to prevent hackers reference the files from their own server. Please reference the file download part**

To maintain the users' icon, we used to call the `Me` API. `Me` API now returns `iconFilePath` which also respect the pattern.

## File downloading

Now as we all know, you can easily get a file represented by `sitename/path/filename`. But you can not download it that way. To download, you need to add some protocol names.

### Full download pattern

Just add:

```url
https://probe.aiursoft.com/Download/Open/{The pattern with url encoded without encoded `/`}
```

For example, if you get a file with the pattern: `mynewsite/myfolder/mysubfolder/img_20190727_143308.jpg`.

- The path of this file is `/`.
- The site name of this file is `mynewsite`.
- The path name of this file is `myfolder/mysubfolder`.
- The file name of this file is `img_20190727_143308.jpg`

Add the head `https://probe.aiursoft.com/Download/Open/`

Get the download address:

```text
https://probe.aiursoft.com/Download/Open/mynewsite/myfolder/mysubfolder/img_20190727_143308.jpg
```

If you add `Download/File` instead of `Download/Open`, the server will return the file as binary, which helps the browser to download.

```text
https://probe.aiursoft.com/Download/File/mynewsite/myfolder/mysubfolder/img_20190727_143308.jpg
```

### Simplified pattern

Just add:

```url
https://{siteName}.aiur.site/{path}/{fileName}
```

For example, if you get a file with the pattern: `mynewsite/myfolder/mysubfolder/img_20190727_143308.jpg`.

- The path of this file is `/`.
- The site name of this file is `mynewsite`.
- The path name of this file is `myfolder/mysubfolder`.
- The file name of this file is `img_20190727_143308.jpg`

Get the download address:

```text
https://mynewsite.aiur.site/myfolder/mysubfolder/img_20190727_143308.jpg
```

## Messages

Kahla message patterns

Video:

```text
[video]mynewsite/myconversation/img_20190727_143308.mp4
```

Audio

```text
[audio]mynewsite/myconversation/img_20190727_143308.mp4
```

File:

```text
[file]mynewsite/myconversation/img_20190727_143308.jpg|myfile.exe(file name)|1.24G(file size)
```

Image:

```text
[img]mynewsite/myconversation/img_20190727_143308.jpg|80|80(For image size)
```

Shared group:

```text
[group]708 (group id)
```

Shared user:

```text
[user]6da0802e-18e4-4a76-99a5-47878dd7b8a5 (user id)
```

## File compressing

The compression algorithm of Probe has been modified. It is now more recommended to use only one parameter to get the compressed image.

For example: specify only `w` or just `h`. Then the value of another dimension, Probe itself calculates, and maintains the aspect ratio of the original image.

These changes I am looking forward to can help simplify some of the fucking code in Kahla.App and save an extra parameter.

Kahla can not calculate the width and height of the real picture, but directly specify only one parameter to quickly complete the rendering of the picture.

Example:

```url
https://probe.aiursoft.com/Download/InSites/usericon/53de617d093040bab113a6508dbcdd19.png?w=100
```

In addition: forcing two parameters to be specified will still cause the Probe to compress with the specified target parameters. However, in this case, the parameter passer must be responsible for maintaining the aspect ratio of the image.

### Messages sent detection

This API may help detect if your message was sent:

- Send message will notify the sender by WebSocket and web push.
- Send message API will auto-clear unread.
- Message ID (Guid type) and send time is determined by the client side. But it can't be far from API calling time.

So, after the send message response, the client can just give the temp message send time and don't have to clear it. And the client don't have to get messages to clear unread.

------

As for `GetMessages` API:

The `skipFrom` argument now is for:

> For which message you want to skip from.  Input that message's GUID.

If the target message not found or argument not passed, it will not skip any message.

That message you specified will not be included in the result.

------

As for `SendMessage` API and `GetMessages` API

Now, sending message request a unique GUID. This will be saved as the message's unique Id.

It must be a valid GUID, for example: `8fe1dd34-7430-4650-8b0a-8587d39dd412`.

Consider code:

```javascript
function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

console.log(uuidv4());
```

And the ID will be returned when returning a message from Json.
