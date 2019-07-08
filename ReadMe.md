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

Setup
-----

This plugin requires KeePass to remain open to function as intended. Ensure that your KeePass lock settings do not close the program as shown below:

<img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/KeePassLockOptions.png?raw=true" width=500/>

Options
-------

The plugin integrates itself into the KeePass settings dialog.

<img src="https://github.com/sirAndros/KeePassWinHello/blob/master/Screenshots/Options.png?raw=true" width=600/>

Available settings:

* Auto prompt (default: true): If enabled a Windows Hello prompt will automatically be opened while unlocking the database as long as it is available.
* Valid time period (default: 24 hours): Choose how long a saved key will be available. Once this period has expired you need to provide your full password to unlock the database again.

Notes
-----

No sensitive information including master passwords for databases are stored by the plugin in a plain text. A database key is encrypted and decrypted using Windows Hello API in order to unlock the database.

Credits
-------

* _Microsoft_ for [Windows Hello][WinHello] technology
* _JanisEst_ and his [KeePassQuickUnlock](https://github.com/JanisEst/KeePassQuickUnlock) for inspiration
