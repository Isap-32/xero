using KeyBoost.Core.Models;
using KeyBoost.Core.Services;
using Xunit;

namespace KeyBoost.Tests;
public sealed class ProfileServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "KeyBoost-profile-tests-" + Guid.NewGuid());
    [Fact]
    public void CreateRenameDeleteArePersisted()
    {
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "settings.json"));
        var service = new ProfileService(config, store);
        var p = service.Create("  My Profile  ", new(2, 5));
        Assert.Equal("My Profile", store.Load().Profiles.Last().Name);
        p = service.Rename(p, "Renamed");
        Assert.Equal("Renamed", store.Load().Profiles.Last().Name);
        config.ActiveProfile = p.Id;
        service.Delete(p);
        Assert.Null(store.Load().ActiveProfile);
        Assert.DoesNotContain(store.Load().Profiles, x => x.Id == p.Id);
    }

    [Fact]
    public void PersistenceFailureRollsBackMemory()
    {
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "settings.json"));
        var service = new ProfileService(config, store);
        var boundList = config.Profiles;
        var original = config.Profiles.ToArray();
        store.BlockWrites();
        Assert.Throws<IOException>(() => service.Create("Unsaved", new(0, 31)));
        Assert.Equal(original, config.Profiles);
        Assert.Same(boundList, config.Profiles);
    }

    [Fact]
    public void CorruptFileCanBePreservedAcrossSubsequentSaves()
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        File.WriteAllText(path, "bad json");
        var store = new ConfigurationStore(path);
        store.PreserveBrokenConfiguration();
        store.Save(new Configuration());
        store.Save(new Configuration());
        Assert.Equal("bad json", File.ReadAllText(Directory.GetFiles(directory, "*.invalid-*.json").Single()));
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }
}
