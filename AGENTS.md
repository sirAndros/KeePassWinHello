# AGENTS.md

## Scope

These instructions apply to the whole repository.

## Project Overview

KeePassWinHello is a KeePass 2 plugin that stores an encrypted database key after the database has been unlocked normally, then uses Windows Hello to authorize later unlocks. The implementation is security-sensitive and Windows-specific; avoid casual behavior changes around key retention, Windows Hello prompts, Credential Manager storage, or KeePass secure desktop handling.

Primary areas:

- `src/KeePassWinHelloExt.cs`: KeePass plugin entry point, host wiring, window hooks, options panel insertion, and unlock/close event routing.
- `src/AuthProviders/`: authentication provider abstraction, Windows Hello/CNG integration, foreground handling for elevated processes, and the dummy XOR provider behind `DUMMY_PROVIDER`.
- `src/KeyManagement/`: KeePass composite key protection, encryption, in-memory storage, Credential Manager storage, and key lifecycle.
- `src/Settings/`: KeePass custom config keys, settings migration, and WinForms options UI.
- `src/Utilities/`: Win32, desktop/window, UAC, and error display helpers.
- `Chocolatey/`: Chocolatey package metadata and install/uninstall scripts.
- Root PowerShell scripts: plugin packaging and deployment helpers.

## Build Setup

This repository does not build from a fresh clone until KeePass is available at `lib\KeePass.exe`. The `lib/` directory is intentionally ignored by git. The README asks for a junction or copy of a KeePass install directory at repository root as `lib`; Travis CI downloads KeePass 2.42.1 into `lib`.

Important project files:

- `src/KeePassWinHello.sln` is the Visual Studio solution.
- `src/KeePassWinHello.csproj` is the legacy .NET Framework 4.0 plugin project used for PLGX packaging. It explicitly lists source files.
- `src/KeePassWinHello.Debug.csproj` is an SDK-style .NET Framework 4.8 helper project with the `Linked KeePass` launch profile and `DebugDummy` configuration.
- Both projects set `LangVersion` to `5`; keep production code compatible with C# 5 and the .NET Framework 4.0 project unless the target framework is intentionally changed.

After adding a new source/resource file, update `src/KeePassWinHello.csproj` explicitly. The SDK-style debug project may include files automatically, but the legacy project and PLGX packaging path will miss new files unless the legacy project knows about them.

## Commands

Commands below assume `lib\KeePass.exe` exists.

CI-style compile command from `.travis.yml`:

```powershell
msbuild src\KeePassWinHello.sln /p:Configuration=Release /p:DefineConstants="MONO"
```

The `MONO` define is used by CI to compile compatibility shims and to skip the post-build packaging target. Without it, normal Debug/Release builds can run `Pack-Plugin.ps1`; Debug builds can also run `Deploy-Plugin.ps1`.

Local Visual Studio debugging:

```text
Open src\KeePassWinHello.sln, choose the KeePassWinHello.Debug project, and run the "Linked KeePass" profile.
```

Package the plugin manually:

```powershell
powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\Pack-Plugin.ps1 -SkipChoco
```

Release packaging without `-SkipChoco` updates the Chocolatey install checksum and verification file and tries to run `choco pack` if Chocolatey is installed.

Deploying locally copies files into a KeePass plugins directory and can modify a developer's local KeePass install. Ask before running `Deploy-Plugin.ps1` unless the user explicitly requested local deployment and supplied/approved the target directory.

## Tests And Verification

There is no test project in this repository. Use build verification plus manual KeePass checks appropriate to the change.

Manual checks should cover the affected paths, especially:

- Normal desktop session, because Windows Hello is not available over RDP.
- First unlock with the normal KeePass master key, then lock/unlock with Windows Hello.
- In-memory storage and Windows Credential Manager storage when key persistence is affected.
- Secure desktop enabled and disabled when unlock prompt handling is affected.
- Elevated KeePass process when foreground/window behavior is affected.
- Options dialog changes, including OK versus Cancel behavior and forced key revocation display.

Do not create or delete persistent Windows Hello/Credential Manager keys during manual testing without user approval.

## Coding Guidelines

- Keep code in the existing C# 5 style: explicit property bodies, `out` parameters, `String.Format` where needed, no pattern matching, no expression-bodied members, no null-propagation, and no string interpolation.
- Preserve .NET Framework 4.0 compatibility in production code. Do not use APIs that only exist in later frameworks unless the legacy project target changes too.
- Treat `DUMMY_PROVIDER`, `MONO`, `DEBUG`, and production Windows Hello paths as separate compile/runtime paths. If you touch `IAuthProvider`, `AuthProviderFactory`, or `XorProvider`, keep the dummy-provider path in sync.
- Keep KeePass reflection defensive. Private members such as `m_pKey`, `m_ioInfo`, and optional close flags can differ by KeePass version; null-check reflected members and preserve fallback behavior.
- Keep Windows Hello prompts tied to `UIContextManager.PushContext` so prompts have the nearest KeePass parent window and a clear user-initiated message.
- Preserve RDP handling. The plugin intentionally disables Windows Hello behavior in terminal server sessions and shows user-facing explanations.
- Use the existing exception taxonomy for provider errors. User cancellation should remain a normal control path; system, unavailable, key-not-found, and invalid-key states have distinct exception types.
- For P/Invoke changes, keep constants and declarations near the related API region, preserve `SafeHandle`/freeing behavior, and route Win32/NCrypt results through the existing `ThrowOnError` helpers.
- For security-sensitive buffers, continue using `ProtectedString`, `ProtectedBinary`, and `MemUtil.ZeroByteArray`. Never log, serialize, or display plaintext KeePass master keys or decrypted key bytes.
- Be careful with persistent storage. `AuthCacheType.Local` uses process memory; `AuthCacheType.Persistent` uses Windows Credential Manager plus a Windows Hello protected key. Switching storage can revoke existing cached keys.
- For WinForms UI changes, keep `OptionsPanel.cs`, `OptionsPanel.Designer.cs`, and `OptionsPanel.resx` consistent. Prefer designer-generated changes for layout/resource updates.
- Avoid unrelated formatting churn in `.Designer.cs`, `.resx`, `.csproj`, `.sln`, Chocolatey metadata, and PowerShell scripts.

## Packaging And Release Notes

- `Pack-Plugin.ps1` derives the plugin version from `src\Properties\AssemblyInfo.cs` when `-Version` is not supplied and updates `keepass.version`.
- PLGX packaging copies from `src/` while excluding build output, solution files, debug project files, Markdown, screenshots, and scripts.
- Chocolatey package metadata expects the built PLGX in `releases/`. The pack script updates `Chocolatey\tools\ChocolateyInstall.ps1` and `Chocolatey\tools\VERIFICATION.txt` checksums when Chocolatey packaging is not skipped.
- Coordinate before changing package IDs, dependency ranges, URLs, versioning, checksums, or release notes.

## When To Ask First

Ask the user before:

- Changing the security model, prompt timing, default key retention period, persistent-storage behavior, or revocation behavior.
- Changing target frameworks, language version, KeePass minimum compatibility, or Windows version assumptions.
- Running scripts that deploy into a local KeePass install or manipulate persistent Windows Hello/Credential Manager state.
- Downloading KeePass or other tools from the network.
- Making release/version/Chocolatey packaging changes that affect published artifacts.

