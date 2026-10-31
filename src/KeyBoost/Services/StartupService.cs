using System;
using Microsoft.Win32;

namespace KeyBoost.Services;
internal static class StartupService
{
    private const string Run = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public static bool Enabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(Run);
            return key?.GetValue("KeyBoost") is string;
        }
    }

    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(Run);
        if (enabled)
            key.SetValue("KeyBoost", "\"" + (Environment.ProcessPath ?? throw new InvalidOperationException("Executable path unavailable.")) + "\"");
        else
            key.DeleteValue("KeyBoost", false);
    }
}
