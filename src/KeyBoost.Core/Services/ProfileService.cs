using KeyBoost.Core.Models;

namespace KeyBoost.Core.Services;
public sealed class ProfileService(Configuration configuration, ConfigurationStore store)
{
    private void Persist(Action mutation)
    {
        var old = configuration.Profiles.ToList();
        var active = configuration.ActiveProfile;
        try
        {
            mutation();
            store.Save(configuration);
        }
        catch
        {
            configuration.Profiles.Clear();
            configuration.Profiles.AddRange(old);
            configuration.ActiveProfile = active;
            throw;
        }
    }

    public Profile Create(string name, KeyboardSettings settings)
    {
        var profile = new Profile(Guid.NewGuid(), name.Trim(), settings);
        profile.Validate();
        Persist(() =>
        {
            if (configuration.Profiles.Count >= 200)
                throw new InvalidDataException("Profile limit reached.");
            configuration.Profiles.Add(profile);
        });
        return profile;
    }

    public Profile Rename(Profile profile, string name)
    {
        var updated = profile with
        {
            Name = name.Trim()
        };
        updated.Validate();
        Persist(() =>
        {
            var index = configuration.Profiles.FindIndex(p => p.Id == profile.Id);
            if (index < 0)
                throw new InvalidDataException("Profile no longer exists.");
            configuration.Profiles[index] = updated;
        });
        return updated;
    }

    public void Delete(Profile profile) => Persist(() =>
    {
        configuration.Profiles.RemoveAll(p => p.Id == profile.Id);
        if (configuration.ActiveProfile == profile.Id)
            configuration.ActiveProfile = null;
    });
    public Profile Import(string path)
    {
        var imported = ConfigurationStore.Import(path);
        return Create(imported.Name, imported.Settings);
    }
}
