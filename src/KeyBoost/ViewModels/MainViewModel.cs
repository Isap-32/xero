using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KeyBoost.Core.Models;
using KeyBoost.Core.Services;

namespace KeyBoost.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly KeyboardSettingsService keyboard;
    private string status = "Ready";
    private int delay, speed;
    private KeyboardSettings? current;
    public Configuration Config { get; }
    public RelayCommand ReadCommand { get; }
    public RelayCommand ApplyCommand { get; }
    public RelayCommand RestoreCommand { get; }

    public event Action<Exception>? OperationFailed;
    public MainViewModel(Configuration config, KeyboardSettingsService keyboard)
    {
        Config = config;
        this.keyboard = keyboard;
        ReadCommand = new(() => Execute(Read));
        ApplyCommand = new(() => Execute(() => Apply(new(Delay, Speed))));
        RestoreCommand = new(() => Execute(Restore), () => Config.Previous is not null);
    }

    public string Status
    {
        get => status;
        set
        {
            status = value;
            Changed();
        }
    }

    public int Delay
    {
        get => delay;
        set
        {
            delay = value;
            Changed();
            Changed(nameof(DelayLabel));
        }
    }

    public int Speed
    {
        get => speed;
        set
        {
            speed = value;
            Changed();
        }
    }

    public string DelayLabel => $"Repeat delay: {(Delay + 1) * 250} ms";
    public string CurrentLabel => current is null ? "Settings not read" : $"{(current.Delay + 1) * 250} ms delay   /   {current.Speed} repeat speed";
    public string ActiveLabel => "Active profile: " + (Config.Profiles.FirstOrDefault(p => p.Id == Config.ActiveProfile && p.Settings == current)?.Name ?? "Custom / current Windows settings");

    public void RefreshProfile() => Changed(nameof(ActiveLabel));
    private void Update(KeyboardSettings settings)
    {
        current = settings;
        Delay = settings.Delay;
        Speed = settings.Speed;
        Changed(nameof(CurrentLabel));
        RefreshProfile();
        RestoreCommand.Refresh();
    }

    public void Read()
    {
        Update(keyboard.Read());
        Status = "Windows settings read successfully";
    }

    public void Apply(KeyboardSettings settings, Guid? profile = null)
    {
        try
        {
            Update(keyboard.Apply(settings, profile));
            Status = "Applied and confirmed by Windows read-back";
        }
        finally
        {
            RestoreCommand.Refresh();
        }
    }

    public void Restore()
    {
        Update(keyboard.Restore());
        Status = "Previous settings restored and confirmed";
    }

    private void Execute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            OperationFailed?.Invoke(ex);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
