using KeyBoost.Core.Models;
using KeyBoost.Core.Services;
using Xunit;

namespace KeyBoost.Tests;
public sealed class KeyboardSettingsTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "KeyBoost-api-tests-" + Guid.NewGuid());
    private sealed class Api : IKeyboardApi
    {
        public KeyboardSettings Value = new(2, 10);
        public bool Reject, Ignore;
        public KeyboardSettings Read() => Value;
        public void Apply(KeyboardSettings settings)
        {
            if (Reject)
                throw new InvalidOperationException("Windows error 5");
            if (!Ignore)
                Value = settings;
        }
    }

    [Fact]
    public void ApplySavesOriginalBeforeChangeAndRestoreSurvivesRestart()
    {
        var api = new Api();
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "config.json"));
        var service = new KeyboardSettingsService(api, store, config);
        var id = config.Profiles[1].Id;
        service.Apply(new(0, 31), id);
        var persisted = store.Load();
        Assert.Equal(new(2, 10), persisted.Previous);
        Assert.Equal(id, persisted.ActiveProfile);
        new KeyboardSettingsService(api, store, persisted).Restore();
        Assert.Equal(new(2, 10), api.Value);
        Assert.Null(store.Load().ActiveProfile);
    }

    [Fact]
    public void RejectedApiLeavesDurableRestoreSnapshot()
    {
        var api = new Api
        {
            Reject = true
        };
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "config.json"));
        Assert.Throws<InvalidOperationException>(() => new KeyboardSettingsService(api, store, config).Apply(new(0, 31)));
        Assert.Equal(api.Value, store.Load().Previous);
        Assert.Null(config.ActiveProfile);
    }

    [Fact]
    public void ReadBackMismatchIsFailure()
    {
        var api = new Api
        {
            Ignore = true
        };
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "config.json"));
        Assert.Throws<InvalidOperationException>(() => new KeyboardSettingsService(api, store, config).Apply(new(0, 31)));
        Assert.Null(config.ActiveProfile);
    }

    [Fact]
    public void NoSnapshotCannotRestore()
    {
        var api = new Api();
        var config = new Configuration();
        var store = new ConfigurationStore(Path.Combine(directory, "config.json"));
        Assert.Throws<InvalidOperationException>(() => new KeyboardSettingsService(api, store, config).Restore());
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }
}
