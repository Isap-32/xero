namespace KeyBoost.Core.Models;
public sealed record KeyboardSettings(int Delay, int Speed)
{
    public void Validate()
    {
        if (Delay is < 0 or > 3 || Speed is < 0 or > 31)
            throw new ArgumentOutOfRangeException(nameof(Delay), "Delay must be 0–3 and speed 0–31.");
    }
}

public sealed record Profile(Guid Id, string Name, KeyboardSettings Settings)
{
    public void Validate()
    {
        if (Id == Guid.Empty || string.IsNullOrWhiteSpace(Name) || Name.Length > 64)
            throw new InvalidDataException("Invalid profile identity or name.");
        if (Settings is null)
            throw new InvalidDataException("Profile settings are required.");
        Settings.Validate();
    }
}

public sealed class Configuration
{
    public int Version { get; set; } = 1;
    public List<Profile> Profiles { get; set; } = Defaults();
    public Guid? ActiveProfile { get; set; }
    public KeyboardSettings? Previous { get; set; }
    public bool LightTheme { get; set; }
    public bool MinimizeToTray { get; set; }
    public bool ConfirmClose { get; set; } = true;

    public static List<Profile> Defaults() => new()
    {
        new(Guid.NewGuid(), "Windows Default", new(1, 31)),
        new(Guid.NewGuid(), "Gaming", new(0, 31)),
        new(Guid.NewGuid(), "Fast Repeat", new(0, 31)),
        new(Guid.NewGuid(), "Custom", new(1, 20))
    };
    public void Validate()
    {
        if (Version != 1 || Profiles is null || Profiles.Count > 200)
            throw new InvalidDataException("Unsupported configuration.");
        foreach (var p in Profiles)
        {
            if (p is null)
                throw new InvalidDataException("Null profile.");
            p.Validate();
        }

        if (Profiles.Select(p => p.Id).Distinct().Count() != Profiles.Count)
            throw new InvalidDataException("Duplicate profile IDs.");
        Previous?.Validate();
    }
}
