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
    private readonly InputSession session = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly Dictionary<Key, Border> keyTiles = new();
    private readonly List<string> history = new();
    private FocusedKeyboardSource? source;
    private void BuildKeyboard()
    {
        string[] rows =
        {
            "Escape F1 F2 F3 F4 F5 F6 F7 F8 F9 F10 F11 F12",
            "Oem3 D1 D2 D3 D4 D5 D6 D7 D8 D9 D0 OemMinus OemPlus Back",
            "Tab Q W E R T Y U I O P OemOpenBrackets Oem6 Oem5",
            "Capital A S D F G H J K L Oem1 OemQuotes Return",
            "LeftShift Z X C V B N M OemComma OemPeriod OemQuestion RightShift",
            "LeftCtrl LWin LeftAlt Space RightAlt RWin RightCtrl Left Up Down Right"
        };
        foreach (var row in rows)
        {
            var panel = new WrapPanel
            {
                Margin = new Thickness(0, 2, 0, 2)
            };
            foreach (var token in row.Split(' '))
            {
                var key = Enum.Parse<Key>(token);
                var tile = new Border
                {
                    CornerRadius = new(4),
                    Margin = new(2),
                    Padding = new(5, 8, 5, 8),
                    MinWidth = key == Key.Space ? 130 : 36,
                    BorderThickness = new(1),
                    BorderBrush = Brushes.DimGray,
                    Child = new TextBlock
                    {
                        Text = Label(key),
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };
                keyTiles[key] = tile;
                panel.Children.Add(tile);
            }

            KeyboardLayout.Children.Add(panel);
        }
    }

    private static string Label(Key key) => key switch
    {
        Key.Oem3 => "`",
        Key.OemMinus => "−",
        Key.OemPlus => "=",
        Key.OemOpenBrackets => "[",
        Key.Oem6 => "]",
        Key.Oem5 => "\\",
        Key.Oem1 => ";",
        Key.OemQuotes => "'",
        Key.OemComma => ",",
        Key.OemPeriod => ".",
        Key.OemQuestion => "/",
        _ => key.ToString().StartsWith("D") && key.ToString().Length == 2 ? key.ToString()[1..] : key.ToString()};
    private void ObserveKey(Key key, bool down)
    {
        var observation = session.Observe((int)key, down, clock.Elapsed.TotalMilliseconds, DateTimeOffset.Now);
        if (observation is not null)
        {
            if (expected.Count > 0)
            {
                foreach (var k in session.Pressed.Where(expected.Contains))
                    observedExpected.Add(k);
                if (expected.IsSubsetOf(session.Pressed))
                    combinationSeen = true;
            }

            var o = observation;
            var line = $"{o.Timestamp:HH:mm:ss.fff} {key, -12} {(down ? "DOWN" : "UP"), -4} hold={o.HoldMs?.ToString("F1") ?? "—"}ms interval={o.IntervalMs?.ToString("F1") ?? "—"}ms taps={o.TapCount}";
            history.Insert(0, line);
            if (history.Count > 250)
                history.RemoveAt(history.Count - 1);
            EventHistory.ItemsSource = null;
            EventHistory.ItemsSource = history;
            PaintKeys();
        }
    }

    private static readonly Brush PressedBrush = new SolidColorBrush(Color.FromRgb(36, 104, 185));
    private void PaintKeys()
    {
        foreach (var pair in keyTiles)
        {
            bool pressed = session.Pressed.Contains((int)pair.Key);
            pair.Value.Background = pressed ? PressedBrush : Brushes.Transparent;
            if (pair.Value.Child is TextBlock label)
            {
                if (pressed)
                    label.Foreground = Brushes.White;
                else
                    label.ClearValue(TextBlock.ForegroundProperty);
            }
        }

        ConcurrentLabel.Text = $"Observed concurrent keys: {session.Pressed.Count} · Peak: {session.Peak}";
    }

    private void FocusTester(object s, MouseButtonEventArgs e) => TestSurface.Focus();
    private void TesterFocused(object s, KeyboardFocusChangedEventArgs e) => InputStatus.Text = "Testing active — only this focused surface receives events";
    private void TesterBlurred(object s, KeyboardFocusChangedEventArgs e) => PauseTester();
    private void PauseTester()
    {
        if (expected.Count > 0)
        {
            var missing = expected.Except(observedExpected).Select(k => ((Key)k).ToString()).ToArray();
            vm.Status = combinationSeen ? "Expected combination observed simultaneously." : "Full combination was not observed simultaneously. Keys never observed: " + (missing.Length == 0 ? "none (keys arrived at different times)" : string.Join(", ", missing)) + ". Hardware rollover cannot be bypassed by software.";
        }

        session.ReleaseAll();
        PaintKeys();
        InputStatus.Text = "Testing paused — click to focus";
    }

    private bool combinationSeen;
    private HashSet<int> expected = new();
    private HashSet<int> observedExpected = new();
    private void CheckCombination(object s, RoutedEventArgs e) => Run(() =>
    {
        expected = ExpectedKeys.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t =>
        {
            if (!Enum.TryParse<Key>(t, true, out var key) || !Enum.IsDefined(key) || key is Key.None or Key.System or Key.DeadCharProcessed or Key.Tab)
                throw new InvalidDataException("Unknown or reserved key: " + t);
            return (int)key;
        }).ToHashSet();
        if (expected.Count == 0)
            throw new InvalidDataException("Enter comma-separated key names, such as A,S,D.");
        observedExpected.Clear();
        session.ReleaseAll();
        PaintKeys();
        combinationSeen = false;
        vm.Status = "Combination armed. Focus the test surface and press the expected keys together; leave focus to see missing keys.";
        TestSurface.Focus();
    });
    private void ClearInput(object s, RoutedEventArgs e)
    {
        session.Clear();
        history.Clear();
        expected.Clear();
        observedExpected.Clear();
        EventHistory.ItemsSource = null;
        PaintKeys();
        vm.Status = "Input results cleared";
    }
}
