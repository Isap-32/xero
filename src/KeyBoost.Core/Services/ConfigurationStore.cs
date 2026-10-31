using System.Text.Json;
using KeyBoost.Core.Models;

namespace KeyBoost.Core.Services;
public sealed class ConfigurationStore(string path)
{
    public string Path { get; } = path;

    private bool writesBlocked;
    public void BlockWrites() => writesBlocked = true;
    public string PreserveBrokenConfiguration()
    {
        if (!File.Exists(Path))
            return "Original path is absent.";
        var backup = Path + ".invalid-" + Guid.NewGuid().ToString("N") + ".json";
        File.Copy(Path, backup);
        return "Original configuration preserved at: " + backup;
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
    public Configuration Load()
    {
        if (!File.Exists(Path))
            return new();
        if (new FileInfo(Path).Length > 1048576)
            throw new InvalidDataException("Configuration exceeds 1 MiB.");
        var config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(Path)) ?? throw new InvalidDataException("Empty configuration.");
        config.Validate();
        return config;
    }

    public void Save(Configuration config)
    {
        if (writesBlocked)
            throw new IOException("Configuration writes are disabled because the damaged file could not be backed up.");
        config.Validate();
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        var temp = Path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(config, JsonOptions));
        if (File.Exists(Path))
            File.Copy(Path, Path + ".bak", true);
        File.Move(temp, Path, true);
    }

    public static Profile Import(string path)
    {
        if (new FileInfo(path).Length > 65536)
            throw new InvalidDataException("Profile exceeds 64 KiB.");
        var p = JsonSerializer.Deserialize<Profile>(File.ReadAllText(path)) ?? throw new InvalidDataException("Empty profile.");
        p.Validate();
        return p with
        {
            Id = Guid.NewGuid()
        };
    }

    public static void Export(string path, Profile profile)
    {
        profile.Validate();
        File.WriteAllText(path, JsonSerializer.Serialize(profile, JsonOptions));
    }
}
