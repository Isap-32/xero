using System;
using System.IO;
using System.Runtime.InteropServices;
using KeyBoost.Core.Services;
using KeyBoost.Native;

namespace KeyBoost.Services;
internal static class DiagnosticsService
{
    private static string Read(Func<object> read)
    {
        try
        {
            return read().ToString() ?? "Unavailable";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex;
        }
    }

    public static string Create(ConfigurationStore store, int eventCount, int peak, string lastError) => $"KeyBoost {typeof(DiagnosticsService).Assembly.GetName().Version}\n" + $"OS: {RuntimeInformation.OSDescription}\nProcess: {RuntimeInformation.ProcessArchitecture}\n" + $"Windows repeat settings: {Read(() => KeyboardNative.Read())}\n" + $"Configuration: {store.Path}\nExists: {File.Exists(store.Path)}\n" + $"Configuration validation: {Read(() => {var c = store.Load(); return File.Exists(store.Path) ? $"Valid v{c.Version}, {c.Profiles.Count} profiles" : "Not yet saved"; })}\n" + $"Observed events retained in memory: {eventCount}\nPeak concurrent: {peak}\n\n" + $"Keyboard device paths (not polling rates):\n{Read(() => KeyboardNative.Devices())}\n\n" + $"Last operation error:\n{lastError}\n\n" + "Input uses focused native WM_KEY messages only. Device enumeration uses Raw Input device APIs; it does not associate an event with a physical device.\n" + "Timing measures application message arrival with a monotonic clock. OS scheduling affects delivery. No physical-to-display latency is measured.\n" + "Reports contain local paths and device identifiers. Event history remains in memory and is not exported.\n";
}
