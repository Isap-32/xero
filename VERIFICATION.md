# KeyBoost build and verification report

Recorded: Saturday, 31 October 2026 at 1:54:13 pm NZDT.

This is a compiled Windows release candidate. It has not been launched or exercised on Windows in this task, and is not certified as production-ready.

## Executed checks

| Check | Observed result |
|---|---|
| Host | Debian 13, Linux x64 |
| SDK | Microsoft .NET SDK 8.0.427; bundled runtime 8.0.33 |
| Frozen restore / solution build | Passed; 4 projects, 0 compiler warnings, 0 errors |
| Automated core suite | 19 passed, 0 failed, 0 skipped |
| Windows API smoke runner | Compiled successfully; not executed |
| Self-contained publish | Passed; win-x64, single-file, compression and native self-extraction enabled |
| Executable inspection | Real PE32+ Windows GUI executable, x86-64 |
| Installer compiler | NSIS 3.11; completed successfully with installer and uninstaller sections |
| Installer inspection | Real PE32 NSIS installer; x86 bootstrap installs the x64 application |
| Application icon | ICO contains 256, 128, 64, 48, 32, 16 pixel sizes |
| Cloud setup script | Executed successfully in this instance |
| GitHub read operation | git ls-remote origin HEAD succeeded with existing proxy authentication |
| Repository whitespace check | git diff --check passed |

The core suite exercises range validation, preset validity, settings/preferences persistence across store restarts, backups, invalid-JSON preservation, import/export identity and range validation, autorepeat suppression, hold/interval calculation, concurrent-key state and focus reset, durable revert snapshots, rejected API and read-back mismatch handling, profile CRUD, persistence rollback (including the UI-bound list identity), and preservation of damaged files across subsequent writes.

Native API failure-path tests use an injected API test double. Their successful results validate service behavior; they do not demonstrate that Windows accepted settings. Input-session tests use deterministic timestamps, not hardware benchmarks.

Evidence: [release log](artifacts/release-build.log) and [TRX test results](artifacts/test-results/core.trx). The final release script preserves command exit statuses and terminates on failure.

## Native Windows checks still required

No Windows machine, interactive Windows desktop, or physical keyboard is attached. The following have **not run**: application launch and rendering; navigation and high-DPI layout; slider interaction; actual SPI changes and read-back/restore; physical down/up and chord detection; profile import/export through native dialogs and restart behavior; theme persistence in the UI; startup registry and tray interactions; diagnostic export from the application; installer completion, installed launch and uninstall. No idle CPU/memory measurements were taken.

[WINDOWS-ACCEPTANCE.md](WINDOWS-ACCEPTANCE.md) lists the exact procedures. The separate C# Windows smoke runner reads real settings/devices and, only with its explicit command-line flag, applies a different pair then restores the original in finally. It leaves a recovery snapshot if interrupted. Neither the runner's native checks nor the manual acceptance rows are marked as passed here.

The app and installer are unsigned. Signing, Windows acceptance, and a supported-runtime maintenance plan remain production distribution requirements. .NET 8 reaches end of support in November 2026.

## Artifacts

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| win-x64/KeyBoost.exe | 73879714 | `0fe921ef705a0b729341cc49a2d4cb5a480ca50385cbe19b8c16dc17d816b3c4` |
| KeyBoost-1.0.0-Setup.exe | 67512892 | `f13d10c7845a0a6c7c986942cdc11c897f61974637d29aaec7178e673845c480` |

The executable comes from dotnet publish; the installer comes from makensis. The portable and source ZIP archives and SHA256SUMS.txt are in artifacts/. The source archive includes the Visual Studio solution, project files, lockfiles, tests, installer, build scripts, documentation, vector/icon assets and redistribution notices. It excludes build caches and Git metadata.

## Reusable environment configuration

Saved draft fields: install_script and start_skill. They capture SDK activation/installation, dependency restoration, build/test commands, packaging, working directories, the existing-checkout guidance, and the Windows execution limitation. No new credentials or network domains were required. The draft save succeeded; it does not publish a snapshot or prove restoration in a new task. Review and save the changes in environment settings, then publish the environment to retain the prepared configuration and filesystem.
