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
    private readonly ProfileService profiles;
    private void RefreshActive() => vm.RefreshProfile();
    private Profile Selected => ProfilePicker.SelectedItem as Profile ?? throw new InvalidOperationException("Select a profile first.");

    private void RefreshProfiles()
    {
        var id = vm.Config.ActiveProfile;
        ProfilePicker.ItemsSource = null;
        ProfilePicker.ItemsSource = vm.Config.Profiles;
        ProfilePicker.SelectedItem = vm.Config.Profiles.FirstOrDefault(p => p.Id == id) ?? vm.Config.Profiles.FirstOrDefault();
        RefreshActive();
    }

    private void ApplyProfile(object s, RoutedEventArgs e) => Run(() => Apply(Selected.Settings, Selected.Id));
    private string NameInput()
    {
        var name = ProfileName.Text.Trim();
        if (name.Length is < 1 or > 64)
            throw new InvalidDataException("Enter a profile name (1–64 characters) on the Profiles page.");
        return name;
    }

    private void CreateProfile(object s, RoutedEventArgs e)
    {
        if (Navigation.SelectedIndex != 3)
        {
            Navigation.SelectedIndex = 3;
            ProfileName.Focus();
            vm.Status = "Enter a name, then choose Create from sliders.";
            return;
        }

        Run(() =>
        {
            var p = profiles.Create(NameInput(), new(vm.Delay, vm.Speed));
            RefreshProfiles();
            ProfilePicker.SelectedItem = p;
            vm.Status = "Profile created";
        });
    }

    private void RenameProfile(object s, RoutedEventArgs e) => Run(() =>
    {
        var p = profiles.Rename(Selected, NameInput());
        RefreshProfiles();
        ProfilePicker.SelectedItem = p;
    });
    private void DeleteProfile(object s, RoutedEventArgs e) => Run(() =>
    {
        var p = Selected;
        if (System.Windows.MessageBox.Show(this, "Delete “" + p.Name + "”?", "Delete profile", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        profiles.Delete(p);
        RefreshProfiles();
    });
    private void ProfileSelected(object s, SelectionChangedEventArgs e)
    {
        if (ProfileName is not null && ProfilePicker.SelectedItem is Profile p)
            ProfileName.Text = p.Name;
    }

    private void ImportProfile(object s, RoutedEventArgs e) => Run(() =>
    {
        var dialog = new OpenFileDialog
        {
            Filter = "KeyBoost profile|*.json"
        };
        if (dialog.ShowDialog(this) != true)
            return;
        var p = profiles.Import(dialog.FileName);
        RefreshProfiles();
        ProfilePicker.SelectedItem = p;
    });
    private void ExportProfile(object s, RoutedEventArgs e) => Run(() =>
    {
        var p = Selected;
        var dialog = new SaveFileDialog
        {
            Filter = "KeyBoost profile|*.json",
            FileName = "KeyBoost-profile.json"
        };
        if (dialog.ShowDialog(this) == true)
            ConfigurationStore.Export(dialog.FileName, p);
    });
}
