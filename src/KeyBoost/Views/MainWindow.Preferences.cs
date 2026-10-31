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
public partial class MainWindow
{
    private System.Windows.Forms.NotifyIcon? tray;
    private void SetTheme()
    {
        var theme = vm.Config.LightTheme ? ApplicationTheme.Light : ApplicationTheme.Dark;
        ApplicationThemeManager.Apply(theme);
        ApplicationAccentColorManager.Apply(Color.FromRgb(57, 129, 233), theme);
    }

    private void ToggleTheme(object s, RoutedEventArgs e)
    {
        LightTheme.IsChecked = !vm.Config.LightTheme;
        PreferenceChanged(s, e);
    }

    private void PreferenceChanged(object s, RoutedEventArgs e)
    {
        if (initializing)
            return;
        Run(() =>
        {
            vm.Config.LightTheme = LightTheme.IsChecked == true;
            vm.Config.MinimizeToTray = TrayToggle.IsChecked == true;
            vm.Config.ConfirmClose = ConfirmToggle.IsChecked == true;
            Save();
            SetTheme();
        });
    }

    private void StartupChanged(object s, RoutedEventArgs e)
    {
        if (initializing)
            return;
        Run(() => StartupService.Set(StartupToggle.IsChecked == true));
        Run(() => StartupToggle.IsChecked = StartupService.Enabled);
    }

    private void ResetPreferences(object s, RoutedEventArgs e) => Run(() =>
    {
        if (System.Windows.MessageBox.Show(this, "Reset theme, tray, close confirmation, and startup preferences? Profiles and keyboard settings are preserved.", "Reset preferences", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        StartupService.Set(false);
        StartupToggle.IsChecked = false;
        LightTheme.IsChecked = false;
        TrayToggle.IsChecked = false;
        ConfirmToggle.IsChecked = true;
        PreferenceChanged(s, e);
    });
    private void OnClosing(object? s, CancelEventArgs e)
    {
        if (vm.Config.ConfirmClose && System.Windows.MessageBox.Show(this, "Close KeyBoost? Applied Windows settings remain active.", "KeyBoost", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            e.Cancel = true;
    }

    private void ShowTray()
    {
        tray ??= new()
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!),
            Text = "KeyBoost",
            Visible = true
        };
        if (tray.ContextMenuStrip is null)
        {
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Open KeyBoost", null, (_, _) => RestoreWindow(this, EventArgs.Empty));
            menu.Items.Add("Exit", null, (_, _) => Close());
            tray.ContextMenuStrip = menu;
        }

        tray.DoubleClick -= RestoreWindow;
        tray.DoubleClick += RestoreWindow;
        tray.Visible = true;
        Hide();
    }

    private void RestoreWindow(object? s, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        if (tray is not null)
            tray.Visible = false;
    }
}
