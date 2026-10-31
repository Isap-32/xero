using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using KeyBoost.Core.Models;
using KeyBoost.Core.Services;
using KeyBoost.Native;
using KeyBoost.Services;
using KeyBoost.ViewModels;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using FluentWindow = Wpf.Ui.Controls.FluentWindow;

namespace KeyBoost.Views;
public partial class MainWindow : FluentWindow
{
    private readonly ConfigurationStore store = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyBoost", "settings.json"));
    private readonly MainViewModel vm;
    private string lastError = "None";
    private bool initializing = true;
    public MainWindow()
    {
        Configuration config;
        try
        {
            config = store.Load();
        }
        catch (Exception ex)
        {
            config = new();
            lastError = ex.Message;
            string recovery;
            try
            {
                recovery = store.PreserveBrokenConfiguration();
            }
            catch (Exception backupError)
            {
                store.BlockWrites();
                recovery = "Configuration writes disabled: " + backupError.Message;
            }

            System.Windows.MessageBox.Show("Configuration could not be loaded. " + ex.Message + "\n" + recovery, "KeyBoost");
        }

        profiles = new(config, store);
        vm = new(config, new KeyboardSettingsService(new WindowsKeyboardApi(), store, config));
        vm.OperationFailed += ex => Run(() => throw ex);
        InitializeComponent();
        DataContext = vm;
        RefreshProfiles();
        BuildKeyboard();
        LightTheme.IsChecked = config.LightTheme;
        TrayToggle.IsChecked = config.MinimizeToTray;
        ConfirmToggle.IsChecked = config.ConfirmClose;
        Run(() => StartupToggle.IsChecked = StartupService.Enabled);
        SetTheme();
        initializing = false;
        Loaded += (_, _) =>
        {
            Run(Read);
            Run(() => DeviceSummary.Text = KeyboardNative.Devices());
        };
        SourceInitialized += (_, _) =>
        {
            source = new(new WindowInteropHelper(this).Handle, () => TestSurface.IsKeyboardFocusWithin && IsActive && InputPage.Visibility == Visibility.Visible);
            source.KeyChanged += ObserveKey;
        };
        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized && vm.Config.MinimizeToTray)
                Run(ShowTray);
        };
        Deactivated += (_, _) => PauseTester();
        Activated += (_, _) =>
        {
            if (TestSurface.IsKeyboardFocusWithin)
                InputStatus.Text = "Testing active — only this focused surface receives events";
        };
        Closing += OnClosing;
        Closed += (_, _) =>
        {
            source?.Dispose();
            tray?.Dispose();
        };
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            lastError = ex.ToString();
            vm.Status = "Error: " + ex.Message;
            System.Windows.MessageBox.Show(this, ex.Message, "KeyBoost — operation failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save() => store.Save(vm.Config);
    private void Read() => vm.Read();
    private void Apply(KeyboardSettings settings, Guid? profile = null) => vm.Apply(settings, profile);
    private void Navigate(object sender, SelectionChangedEventArgs e)
    {
        if (OverviewPage is null)
            return;
        var pages = new FrameworkElement[]
        {
            OverviewPage,
            KeyboardPage,
            InputPage,
            ProfilesPage,
            DiagnosticsPage,
            SettingsPage
        };
        for (int i = 0; i < pages.Length; i++)
            pages[i].Visibility = i == Navigation.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
        PageTitle.Text = (Navigation.SelectedItem as ListBoxItem)?.Tag?.ToString() ?? "KeyBoost";
        session.ReleaseAll();
        PaintKeys();
        if (Navigation.SelectedIndex == 4)
            Run(UpdateDiagnostics);
    }
}
