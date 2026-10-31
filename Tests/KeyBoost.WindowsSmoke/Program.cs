using System.Text.Json;
using KeyBoost.Native;
using KeyBoost.Core.Models;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("This runner requires real Windows.");
    return 2;
}

var results = new List<object>();
int failed = 0;
void Check(string name, Action action)
{
    try
    {
        action();
        results.Add(new { name, outcome = "passed", error = (string? )null });
        Console.WriteLine("PASS " + name);
    }
    catch (Exception ex)
    {
        failed++;
        results.Add(new { name, outcome = "failed", error = ex.ToString() });
        Console.Error.WriteLine("FAIL " + name + ": " + ex.Message);
    }
}

Check("Read real Windows keyboard settings", () => KeyboardNative.Read().Validate());
Check("Enumerate keyboard device interfaces", () => Console.WriteLine(KeyboardNative.Devices()));
if (args.Contains("--allow-settings-change"))
{
    Check("Apply, read-back, restore real Windows settings", () =>
    {
        var original = KeyboardNative.Read();
        // Persist the exact original first so it remains recoverable if the test process is interrupted.
        var snapshot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyBoost", "windows-smoke-original.json");
        Directory.CreateDirectory(Path.GetDirectoryName(snapshot)!);
        if (File.Exists(snapshot))
            throw new InvalidOperationException("A previous smoke-test recovery snapshot exists: " + snapshot + ". Inspect it before running another settings test.");
        File.WriteAllText(snapshot, JsonSerializer.Serialize(original));
        try
        {
            var requested = new KeyboardSettings(original.Delay == 0 ? 1 : 0, original.Speed == 31 ? 20 : 31);
            KeyboardNative.Apply(requested);
            if (KeyboardNative.Read() != requested)
                throw new InvalidOperationException("Read-back mismatch.");
        }
        finally
        {
            KeyboardNative.Apply(original);
            if (KeyboardNative.Read() != original)
                throw new InvalidOperationException("Original settings were not restored. Snapshot: " + snapshot);
            File.Delete(snapshot);
        }
    });
}
else
{
    results.Add(new { name = "Apply, read-back, restore real Windows settings", outcome = "not run", error = "Requires --allow-settings-change; closes and restores user setting changes in finally." });
}

var output = Path.GetFullPath("windows-smoke-results.json");
File.WriteAllText(output, JsonSerializer.Serialize(new { executedOn = DateTimeOffset.Now, os = System.Runtime.InteropServices.RuntimeInformation.OSDescription, results }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("Results: " + output);
return failed == 0 ? 0 : 1;
