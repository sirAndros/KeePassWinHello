# [KeePassWinHello 3.2](https://github.com/sirAndros/KeePassWinHello/releases/tag/v3.2)

Fixed following issues:

* [#42](https://github.com/sirAndros/KeePassWinHello/issues/42) - Handle inner TPM error;
* [#51](https://github.com/sirAndros/KeePassWinHello/issues/51) - Fallback to default unlock on remote desktop (Windows Hello is not available for remote sessions);
* [#56](https://github.com/sirAndros/KeePassWinHello/issues/56) - Access denied for `AllowSetForegroundWindow`;
* [#60](https://github.com/sirAndros/KeePassWinHello/issues/60) - Fallback to using local key if the persistent one is absent;
* [#63](https://github.com/sirAndros/KeePassWinHello/issues/63) - Address `NTE_INVALID_HANDLE` error;
* [#68](https://github.com/sirAndros/KeePassWinHello/issues/68) - Fix TPM issue related to hibernation;
* [#69](https://github.com/sirAndros/KeePassWinHello/issues/69) - Address `NTE_BAD_KEYSET` error;
* [#71](https://github.com/sirAndros/KeePassWinHello/issues/71) - Address `NTE_BAD_DATA` error;
* [#72](https://github.com/sirAndros/KeePassWinHello/issues/72) - Handle inner `NCryptEncrypt` error;
* [#77](https://github.com/sirAndros/KeePassWinHello/issues/77) - Address `TPM_20_E_SIZE` error;

And also simplified bugs reporting through dedicated button in error message dialog.
