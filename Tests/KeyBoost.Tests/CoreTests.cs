using KeyBoost.Core.Models;
using KeyBoost.Core.Services;
using Xunit;

namespace KeyBoost.Tests;
public sealed class CoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "KeyBoost-tests-" + Guid.NewGuid());
    private string PathName => Path.Combine(directory, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(4, 31)]
    [InlineData(0, -1)]
    [InlineData(3, 32)]
    public void RejectsUnsupportedSettings(int delay, int speed) => Assert.Throws<ArgumentOutOfRangeException>(() => new KeyboardSettings(delay, speed).Validate());
    [Fact]
    public void AllPresetsAreSupported()
    {
        foreach (var p in Configuration.Defaults())
            p.Validate();
    }

    [Fact]
    public void ProfilesAndPreferencesSurviveRestart()
    {
        var store = new ConfigurationStore(PathName);
        var config = store.Load();
        config.LightTheme = true;
        config.ActiveProfile = config.Profiles[1].Id;
        config.Previous = new(2, 12);
        store.Save(config);
        var read = new ConfigurationStore(PathName).Load();
        Assert.Equal(config.ActiveProfile, read.ActiveProfile);
        Assert.True(read.LightTheme);
        Assert.Equal(config.Previous, read.Previous);
        Assert.Equal(config.Profiles, read.Profiles);
    }

    [Fact]
    public void SavesBackupBeforeReplacement()
    {
        var s = new ConfigurationStore(PathName);
        var c = new Configuration();
        s.Save(c);
        c.LightTheme = true;
        s.Save(c);
        Assert.False(new ConfigurationStore(PathName + ".bak").Load().LightTheme);
    }

    [Fact]
    public void InvalidConfigurationIsPreserved()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(PathName, "{broken");
        Assert.Throws<System.Text.Json.JsonException>(() => new ConfigurationStore(PathName).Load());
        Assert.Equal("{broken", File.ReadAllText(PathName));
    }

    [Fact]
    public void ProfileRoundTripUsesNewIdentity()
    {
        Directory.CreateDirectory(directory);
        var p = Configuration.Defaults()[0];
        ConfigurationStore.Export(PathName, p);
        var read = ConfigurationStore.Import(PathName);
        Assert.Equal(p.Settings, read.Settings);
        Assert.Equal(p.Name, read.Name);
        Assert.NotEqual(p.Id, read.Id);
    }

    [Fact]
    public void ImportedProfileCannotUseOutOfRangeSettings()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(PathName, "{\"Id\":\"ae63f139-d881-4d23-83c7-2137d6bbaf54\",\"Name\":\"bad\",\"Settings\":{\"Delay\":5,\"Speed\":31}}");
        Assert.Throws<ArgumentOutOfRangeException>(() => ConfigurationStore.Import(PathName));
    }

    [Fact]
    public void InputIgnoresAutoRepeatAndMeasuresHoldAndInterval()
    {
        var s = new InputSession();
        var now = DateTimeOffset.UtcNow;
        Assert.Equal(1, s.Observe(65, true, 100, now)!.TapCount);
        Assert.Null(s.Observe(65, true, 120, now));
        Assert.Equal(50, s.Observe(65, false, 150, now)!.HoldMs);
        var next = s.Observe(65, true, 220, now)!;
        Assert.Equal(120, next.IntervalMs);
        Assert.Equal(2, next.TapCount);
    }

    [Fact]
    public void ConcurrentKeysAndBlurDoNotInventKeyUps()
    {
        var s = new InputSession();
        var now = DateTimeOffset.UtcNow;
        s.Observe(65, true, 1, now);
        s.Observe(66, true, 2, now);
        Assert.Equal(2, s.Peak);
        s.ReleaseAll();
        Assert.Empty(s.Pressed);
        Assert.Null(s.Observe(65, false, 20, now));
        s.Clear();
        Assert.Equal(0, s.Peak);
    }
}
