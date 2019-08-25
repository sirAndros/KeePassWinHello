Quick unlock with Windows Hello for KeePass 2
=============================================

This plugin for [KeePass 2][KeePass] password manager is intended for fast authorization to a database after its first unlock using [Windows Hello technology][WinHello].

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

Tested on Microsoft Surface with KeePass 2.39.1.

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

By default this plugin holds an encrypted master password in memory and removes it upon KeePass closing. In order to be able to unlock your database via Windows Hello authentication in between KeePass launches you may check "Store keys in the Windows Credential Manager" on in the Options dialog. This will prompts you for creating a persistent key signed with your biometry via Windows Hello. The key is used to encrypt master passwords for securely storing them in the Windows Credential Manager.

Options
-------

The plugin integrates itself into the KeePass settings dialog.

<img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/Options.png?raw=true" width=600/>

Available settings:

* Valid time period (default: 24 hours): Choose how long a saved key will be available. Once this period has expired you need to provide your full password to unlock the database again.
* Storage location: Choose where to hold encrypted master passwords - in the KeePass process memory (by default) or in the Windows Credential Manager.
* Revoke all: Allows you to delete all stored keys.

Notes
-----

No sensitive information including master passwords for databases are stored by the plugin in a plain text. A database key is encrypted and decrypted using Windows Hello API in order to unlock the database.

Credits
-------

* _Microsoft_ for [Windows Hello][WinHello] technology
* _JanisEst_ and his [KeePassQuickUnlock](https://github.com/JanisEst/KeePassQuickUnlock) for inspiration
