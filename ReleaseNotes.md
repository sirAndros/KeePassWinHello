# [KeePassWinHello 3.3](https://github.com/sirAndros/KeePassWinHello/releases/tag/v3.3)

Fixed following issues:

- Force keys revoking confirmation: now all settings able to be approved by "OK" or canceled.
- Show visually when keys will be removed.
- fix [#93](https://github.com/sirAndros/KeePassWinHello/issues/93) - retry `WINBIO_E_DATA_PROTECTION_FAILURE`
- fix [#92](https://github.com/sirAndros/KeePassWinHello/issues/92) - force set non-secure desktop to find and close warning from KeePass related to escaping from a secure desktop to make it possible to prompt WinHello (it can't be run on secure desktop)
- fix [#97](https://github.com/sirAndros/KeePassWinHello/issues/97) - fix accessing a main KeePass window handle from different thread
- fix [#48](https://github.com/sirAndros/KeePassWinHello/issues/48) - using nearest keepass window as parent to attach (instead of main window)
- fix [#54](https://github.com/sirAndros/KeePassWinHello/issues/54) - using nearest keepass window as parent to attach (instead of main window)

- *lower probability of [#25](https://github.com/sirAndros/KeePassWinHello/issues/25): Windows Hello Prompt Hidden*
- *lower probability of [#86](https://github.com/sirAndros/KeePassWinHello/issues/86): Windows Hello window not in focus when KeePass opened through a shortcut*

Also check out the security notice: https://github.com/sirAndros/KeePassWinHello/tree/b168c5a0847ff6192ea08e92dd3cc9bf458b2ef3?tab=readme-ov-file#security-notice
