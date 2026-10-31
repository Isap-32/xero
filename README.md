# KeyBoost

A native C# / WPF keyboard control and diagnostics utility for Windows 10/11 x64. The release is self-contained: users do not need .NET, Visual Studio, Python, or an internet connection to run it. The interface uses WPF UI and Microsoft Fluent System Icons.

**Verification status:** compiled and packaged on Linux; automated core tests executed there. Windows UI, real keyboard interaction, installation, and uninstallation have not been executed in this environment. See [VERIFICATION.md](VERIFICATION.md) before treating this as a production-verified release.

## Install or run

- Installer: `artifacts/KeyBoost-1.0.0-Setup.exe`. Installs for the current user under `%LOCALAPPDATA%\Programs\KeyBoost` and adds a Start menu shortcut and an Apps uninstall entry. Close KeyBoost before installing an update or uninstalling.
- Portable: `artifacts/KeyBoost-1.0.0-portable-win-x64.zip`. Extract the whole archive, then run `KeyBoost.exe`.
- The binaries are unsigned. No signing certificate was available. Windows may display its unrecognized publisher warning.
- Uninstall from Windows Settings → Apps, or run `Uninstall.exe` in the installation directory. User profiles and backups are retained. Applied Windows settings also remain; use **Restore previous settings** before uninstalling if you want to revert them.

## Use

**Overview:** read the current Windows repeat configuration, choose and apply a preset, see keyboard device interface paths, or open diagnostics.

**Keyboard Settings:** move the delay slider (Windows values 0–3, nominally 250–1000 ms) and speed slider (0–31, approximately 2.5–30 characters/sec). Apply saves the exact previous Windows settings first, invokes `SystemParametersInfo`, and reads the result back. Each new successful application replaces the previous-settings snapshot; Restore uses that snapshot and confirms the result. It does not reset to a guessed factory value. A partial API failure also retains the snapshot for recovery.

These controls affect Windows character repetition. They do not change a keyboard's physical actuation, USB polling frequency, hardware rollover, or the direct input latency of most games. Preset names are conveniences, not performance claims. Gaming and Fast Repeat intentionally use the same supported fast-repeat values.

**Input Tester:** click the blue outlined test surface. Presses highlight the compact US logical layout; all delivered keys appear in the rolling history, including keys absent from the diagram. Hold duration and down-to-down intervals use a monotonic clock. OS autorepeat does not increment tap counts. History retains 250 events in memory; it is never automatically saved or exported. Tab/Shift+Tab leave the test surface and are reserved for navigation. Windows-reserved shortcuts may be intercepted by the OS. A deactivated window pauses capture and clears held keys without inventing key-up events.

To check a combination, enter Windows/WPF logical key names separated by commas (for example `A,S,D` or `LeftCtrl,A`). Choose **Check expected combination**, press the keys together, then leave the surface. The status reports whether the full combination was observed simultaneously and which expected keys never arrived. Clear Results resets the test. An absent key cannot be conclusively attributed to hardware: the OS and shortcuts may also intercept input. Software cannot bypass hardware rollover limits.

Timing measures delivery to this application's focused window, not physical-to-display latency. Device enumeration uses the Raw Input device-list APIs. Input events use native focused `WM_KEYDOWN`, `WM_KEYUP`, and system-key messages; no global hook or background Raw Input subscription is installed. Left/right modifiers are distinguished using scan codes and extended-key flags.

**Profiles:** enter a name and create from the slider values; rename, apply, delete, import, or export JSON. Imports are limited to 64 KiB and validate names, identity, and Windows value ranges. Import assigns a new ID. Up to 200 profiles are supported. Windows Default is a conventional preset (delay 1, speed 31), not an original-settings backup.

**Diagnostics:** refresh or export real OS/app versions, current settings, device paths, configuration validation, event counts, and errors. A failed API is represented as an error in the report. Reports contain local paths and hardware identifiers; inspect them before sharing. They do not contain the captured key history.

**Settings:** theme and preferences persist locally. Start with Windows is opt-in and uses the current user's Run registry key. With Minimize to tray enabled, minimize hides the window; double-click the icon or use its Open/Exit menu. Closing is separate from minimizing. Optional close confirmation explains that applied Windows settings remain active. Reset preferences preserves profiles and Windows settings.

## Storage and recovery

`%LOCALAPPDATA%\KeyBoost\settings.json` stores profiles, preferences, active-profile ID, and the previous keyboard snapshot. Writes replace through a temporary file; `settings.json.bak` retains the prior configuration. Invalid configurations are copied to a unique `.invalid-<id>.json` before subsequent writes. If preservation fails, configuration writes are disabled and an error is shown. No credentials are required or stored.

## Build

Use the .NET SDK pinned in `global.json` (8.0.427), with the Windows desktop targeting pack restored by NuGet. Building from source needs Internet access to NuGet on first restore. Visual Studio 2022 with the .NET desktop workload can open `KeyBoost.sln`; Visual Studio is optional for CLI builds.

```powershell
dotnet restore KeyBoost.sln --locked-mode
dotnet build KeyBoost.sln -c Release --no-restore
dotnet test Tests/KeyBoost.Tests -c Release --no-build --logger "trx;LogFileName=core.trx" --results-directory artifacts/test-results
dotnet publish src/KeyBoost/KeyBoost.csproj -c Release -r win-x64 --self-contained true -p:RestoreLockedMode=true -o artifacts/win-x64
makensis installer/KeyBoost.nsi
```

`PublishSingleFile`, compression, and native self-extraction are configured in the application project. Do not enable trimming for WPF. The single executable includes the .NET runtime; native runtime files may be extracted to the user's temporary directory at startup. NSIS 3 is needed only for building the installer. `scripts/release.ps1` automates these commands on Windows; `scripts/release.sh` uses this cloud instance's locally installed SDK and NSIS.

The isolated Linux cloud workflow is `bash scripts/setup-cloud.sh`, followed by `bash scripts/release.sh`. It cross-compiles Windows binaries and runs the cross-platform core suite. It cannot execute WPF. Use the existing checkout; each cloud task is already isolated and no Git worktree is needed.

On a real Windows test account, close KeyBoost and run the native smoke runner:

```powershell
dotnet run --project Tests/KeyBoost.WindowsSmoke -c Release -- --allow-settings-change
```

This deliberately changes repeat settings, verifies read-back, and restores the exact originals in `finally`. It writes a recovery snapshot before changing anything and produces `windows-smoke-results.json`. Omitting the flag executes read/device checks only and explicitly records the setting test as not run. This runner has been compiled here but **not executed on Windows**. Continue with the [Windows acceptance checklist](WINDOWS-ACCEPTANCE.md); the smoke runner does not validate UI, physical keyboard input, or installation.

## Structure

- `src/KeyBoost`: WPF views, command-based view model, Win32 adapters, startup/tray integration, branding and themes.
- `src/KeyBoost.Core`: validated models, JSON storage, settings transaction service with an injectable API, and focused input-session measurement.
- `Tests/KeyBoost.Tests`: cross-platform behavior and failure-path tests.
- `Tests/KeyBoost.WindowsSmoke`: explicit native Windows API verification runner.
- `installer`: per-user NSIS installer and uninstaller.
- `licenses`, `THIRD-PARTY-NOTICES.txt`: dependency redistribution notices.

No network requests, drivers, global key hooks, kernel modifications, game injection, or periodic polling occur in the application. .NET 8 reaches end of support in November 2026; update the target framework and bundled runtime for ongoing distribution beyond that date.
