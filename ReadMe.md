Quick unlock with Windows Hello for KeePass 2
=============================================

[![Build Status](https://travis-ci.org/sirAndros/KeePassWinHello.svg?branch=master)](https://travis-ci.org/sirAndros/KeePassWinHello)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=sirAndros_KeePassWinHello&metric=alert_status)](https://sonarcloud.io/dashboard?id=sirAndros_KeePassWinHello)
[![GitHub All Releases](https://img.shields.io/github/downloads/sirAndros/KeePassWinHello/total)](https://github.com/sirAndros/KeePassWinHello/releases)
[![Chocolatey](https://img.shields.io/chocolatey/dt/keepass-plugin-winhello?label=chocolatey)](https://chocolatey.org/packages/keepass-plugin-winhello)



This plugin for [KeePass 2][KeePass] password manager is intended for fast authorization with pin or biometrics to a database after its first unlock using [Windows Hello technology][WinHello].

[KeePass]: https://keepass.info/
[WinHello]: https://support.microsoft.com/en-us/help/17215/windows-10-what-is-hello

Usage
-----

With this plugin you may:

1. Unlock your database with your masterkey/keyfile/other provider;

    <img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/KeePassPrompt.png?raw=true" width=500/>
2. Lock the database (for example, applying autolock on minimize);
3. When you try to unlock it again, if Windows Hello is available on your system and active for the database, a Windows Hello prompt will be shown over a classic KeePass unlock prompt;

    <img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/Hello1.png?raw=true" width=500/>
4. Profit!

Systems Requirements
--------------------

This plugin relies on Windows Hello API and its [requirements][WinHelloReq].

There are some known issues with Windows Hello reported by community.
Please, check [here](https://github.com/sirAndros/KeePassWinHello/wiki/Windows-Hello-issues) before write issue.

Tested on Microsoft Surface Pro 2017 with KeePass 2.39.1 and 2.42.1.

[WinHelloReq]: https://www.microsoft.com/en-US/windows/windows-10-specifications

How to Install
--------------

Place [KeePassWinHelloPlugin.plgx][binLink] into `Plugins` folder in your KeePass installation
*(by default is `C:\Program Files (x86)\KeePass Password Safe 2`)*.

[binLink]: https://github.com/sirAndros/KeePassWinHello/releases "Plugin Releases"

Or you can use [Chocolatey](https://chocolatey.org/packages/keepass-plugin-winhello) to install it in a more automated manner:

``` powershell
choco install keepass-plugin-winhello
```

Key storage
-----------

By default, this plugin holds an encrypted master password in memory and removes it upon KeePass closing.
In this scenario, the plugin asks Windows to issue a disposable encryption key in its security subsystem (NGC) and then uses it by name to encrypt and decrypt all parts of the composite keys of databases.
The plugin gets and stores only an ID of this key and the encrypted data itself. Encryption and decryption are performed by Windows with a WinHello prompt.

In order to unlock your database via Windows Hello authentication between KeePass launches, you may check "Store keys in the Windows Credential Manager" in the Options dialog.
This will prompt you to create a persistent key signed with your biometry via Windows Hello.
The key is used to encrypt master passwords for securely storing them in the Windows Credential Manager.
In this scenario, the key has a constant name based on the current user ID, and the plugin does **not** store in memory either the key name or the encrypted data.

Options
-------

The plugin integrates itself into the KeePass settings dialog.

<img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/Options.png?raw=true" width=600/>

Available settings:

* Valid time period (default: 24 hours): Choose how long a saved key will be available. Once this period has expired you need to provide your full password to unlock the database again.
* Storage location: Choose where to hold encrypted master passwords - in the KeePass process memory (by default) or in the Windows Credential Manager.
* Revoke all: Allows you to delete all stored keys.

All changes will be applied only after `OK` button press.
You can cancel the modifications using `Cancel` button instead.

Security Notice
---------------

As you should never approve any process elevation (run as admin) if you don't trust an application (because otherwise they can do almost anything), you should never sign Windows Hello prompt if you did not request it, especially when using the persistent storage.
In those cases your both passwords and PC are at risk.

Our plugin prompts you to authorize Windows Hello only in the following cases:

* You prompted to decrypt KeePass database.
* You changed the in-memory storage to the persistent one.
* If one of the operations above failed due to a known recoverable internal Windows Hello problem we could retry the prompt with an appropriate message.
_Warning: if an action being requested by you was actually succeeded, this one can be phishing "retry" and you should also cancel it if you're not sure._

So it works like a rule of thumb: just like you cancel an unintended UAC dialog, you should cancel an unintended Windows Hello prompt.

Notes
-----

No sensitive information including master passwords for databases are stored by the plugin in a plain text. A database key is encrypted and decrypted using Windows Hello API in order to unlock the database.

Development
-----------

Create junction/copy folder with KeePass.exe to root of the repository as folder named "lib".
Debug "KeePassWinHello.Debug" project with "Linked KeePass" launch profile.
If you add some file - don't forget to add it to `KeePassWinHello.csproj`, otherwise it won't be included in plgx on publish.

Credits
-------

* _Microsoft_ for [Windows Hello][WinHello] technology
* _JanisEst_ and his [KeePassQuickUnlock](https://github.com/JanisEst/KeePassQuickUnlock) for inspiration
