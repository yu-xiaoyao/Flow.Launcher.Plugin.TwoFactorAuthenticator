Flow.Launcher.Plugin.TwoFactorAuthenticator
==================
[TOC]


A Two Factor Authenticator Code Generator for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).

# Two Factor Authenticator/两步认证插件

## Support OTP Type

- [x] TOTP
- [ ] HOTP. (Not support yet)

## Import Support Schema

1. otpauth://
2. otpauth-migration:// : Google Authenticator App Shared URL



## Usage
```
    2fa <arguments>
```

![](Resources/usage.png)

**Input Filter:**
![](Resources/usage-filter.png)


### Copy Code
> Select Item Press `Enter` or `Click` to Copy Code

### Preview Panel
![](Resources/preview.png)


### Context Menu
![](Resources/context-menu.png)



## Settings

![Settings](Resources/settings-manage.png)

### CheckBox Settings

- `Show Copy Notification`: Show a notification when the code is copied to the clipboard.
- `Enable Pinyin Search`: When Use Chinese, enable Pinyin search. 中文接音搜索.
- `Is Search Name`: Is Filter by Name Column.
- `Is Search Issuer`: Is Filter by Issuer Column.

### Import
#### 1.Import From Image
> Support Image File Format: PNG, JPG, JPEG

QR Code Image Content String Support:
1. otpauth://
2. otpauth-migration://

#### 2. Import From Clipboard
1. Clipboard String. `otpauth://` or `otpauth-migration://`
2. QrCode Image File. Image Content StringSupport: `otpauth://` or `otpauth-migration://`

#### Support OTP URL Schema
1. otpauth://
2. otpauth-migration:// : Google Authenticator App Shared URL

### Add OTP

![](Resources/add-otp.png)


### Edit OTP
1. Select Edit Item
![](Resources/edit-input.png)
2. Open Context Menu, and Select Edit
![](Resources/edit-otp.png)

> PS: After add or Edit Must Click `Save And Reload Settings`

## Google Protobuf for OTP Migration URL Protocol

> custom Generate Migration.cs file

```shell
protoc --csharp_out=./Migration Migration/migration.proto
```