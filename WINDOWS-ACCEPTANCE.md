# Windows acceptance record — not executed

Run on a disposable Windows 10/11 x64 user account with a real keyboard, without elevation. Record the OS build, display scale, device model, artifact SHA-256, actual outcome, and evidence for every row. A checked row requires an observed result. This file is an execution checklist, not a claim that the steps passed.

| Check | Procedure / expected result | Result here |
|---|---|---|
| Launch | Run the portable executable offline. Main window opens, no errors. | Not run — Linux host |
| Layout / DPI | Inspect at 100%, 150%, 200%; resize from 920×640 to 1920×1080; maximize/restore and drag/snap. No clipped controls. | Not run |
| Navigation | Open all six pages by mouse and keyboard. Focus remains visible. | Not run |
| Sliders | Keyboard Settings: adjust both sliders using mouse and arrow keys, including 0/3 and 0/31 bounds. Values stay integral. | Not run |
| Native API | Close the app; run `dotnet run --project Tests/KeyBoost.WindowsSmoke -c Release -- --allow-settings-change`. Inspect its JSON results. Exact original settings restored. | Runner compiled, not executed |
| App apply/read-back | Record current values; apply another pair, Read Current Settings, compare with Windows keyboard properties. No success if the API rejects. | Not run |
| Restore | Restart after applying; Restore Previous Settings returns the exact pre-apply pair. | Not run |
| Focused input | Focus tester; press/release A, both Shift keys, Ctrl, Alt, arrows, function keys. Down/up and highlights match. Repeats don't increment taps. | Not run |
| Timing | Hold then tap a key several times; hold and interval fields show measured elapsed values. No latency claims. | Not run |
| Privacy / focus | Type into another app, minimize KeyBoost, or switch pages. No new events. Refocus: no stuck highlighted keys. Tab leaves the tester. | Not run |
| Combinations | Arm A,S,D; press together then leave focus: observed. Repeat while omitting D: reports missing D. Reset clears history, peak and expected set. | Not run |
| Profiles | Create, rename, apply, export, delete, import; restart and verify persistence and selected active profile. Try malformed JSON and out-of-range settings. Errors leave system settings unchanged. | Not run |
| Theme | Switch theme twice, restart, verify persistence and readable key highlights. | Not run |
| Tray / close | Enable tray, minimize, reopen by icon and menu, exit; exercise confirm-close yes/no. | Not run |
| Startup | Opt in, inspect current user's Run entry and restart/log in. Opt out removes it. No admin prompt. | Not run |
| Diagnostics | Refresh/export; compare OS, setting values and devices with this system. Inspect errors and configuration status. | Not run |
| Installer | On a clean account, run Setup. Installs per user without elevation; Start menu shortcut and Apps entry exist. | Not run |
| Installed launch | Disconnect network and launch from Start menu. Repeat apply/read-back/restore. | Not run |
| Update protection | With KeyBoost running, installer refuses overwrite. Close it and retry. | Not run |
| Uninstall | Close app; uninstall from Apps. Program and shortcut removed; startup entry removed; unrelated files and profile/backup data retained. | Not run |
| Idle efficiency | With tester unfocused, inspect CPU and memory over 60 seconds; record actual numbers. No periodic input polling. | Not run; no invented measurements |

The executable and installer are unsigned. Code signing and this Windows acceptance pass remain release gates for a production distribution. No Windows execution service is attached to this cloud task.
