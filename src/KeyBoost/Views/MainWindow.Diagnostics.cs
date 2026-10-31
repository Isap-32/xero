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
    private void OpenDiagnostics(object s, RoutedEventArgs e) => Navigation.SelectedIndex = 4;
    private void UpdateDiagnostics() => DiagnosticText.Text = DiagnosticsService.Create(store, history.Count, session.Peak, lastError);
    private void RefreshDiagnostics(object s, RoutedEventArgs e) => Run(UpdateDiagnostics);
    private void ExportDiagnostics(object s, RoutedEventArgs e) => Run(() =>
    {
        UpdateDiagnostics();
        var dialog = new SaveFileDialog
        {
            Filter = "Text report|*.txt",
            FileName = "KeyBoost-diagnostics.txt"
        };
        if (dialog.ShowDialog(this) == true)
            File.WriteAllText(dialog.FileName, DiagnosticText.Text);
    });
}
