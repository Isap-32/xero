using System;
using System.Threading;
using System.Windows;

namespace KeyBoost;
public partial class App : Application
{
    private Mutex? instance;
    protected override void OnStartup(StartupEventArgs e)
    {
        instance = new Mutex(true, @"Local\KeyBoost.Desktop.1", out bool created);
        if (!created)
        {
            MessageBox.Show("KeyBoost is already running. Check its window or system tray.", "KeyBoost");
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instance?.Dispose();
        base.OnExit(e);
    }
}
