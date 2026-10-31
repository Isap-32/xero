using KeyBoost.Core.Models;

namespace KeyBoost.Core.Services;
public interface IKeyboardApi
{
    KeyboardSettings Read();
    void Apply(KeyboardSettings settings);
}

/// <summary>Saves the revert snapshot before touching Windows, including when an API partially fails.</summary>
public sealed class KeyboardSettingsService(IKeyboardApi api, ConfigurationStore store, Configuration configuration)
{
    public KeyboardSettings Read() => api.Read();
    public KeyboardSettings Apply(KeyboardSettings requested, Guid? profileId = null)
    {
        requested.Validate();
        configuration.Previous = api.Read();
        store.Save(configuration); // Failure to create a durable backup prevents the system change.
        api.Apply(requested);
        var actual = api.Read();
        if (actual != requested)
            throw new InvalidOperationException("Windows read-back differs from the requested settings. Use Restore previous settings to revert.");
        configuration.ActiveProfile = profileId;
        store.Save(configuration);
        return actual;
    }

    public KeyboardSettings Restore()
    {
        var previous = configuration.Previous ?? throw new InvalidOperationException("No previous settings snapshot is available.");
        api.Apply(previous);
        var actual = api.Read();
        if (actual != previous)
            throw new InvalidOperationException("Windows did not accept the previous settings.");
        configuration.ActiveProfile = null;
        store.Save(configuration);
        return actual;
    }
}
