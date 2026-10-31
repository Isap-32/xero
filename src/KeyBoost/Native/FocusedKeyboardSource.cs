using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace KeyBoost.Native;
/// <summary>Observes this window's message queue only. No global hook or background Raw Input registration.</summary>
internal sealed class FocusedKeyboardSource : IDisposable
{
    private readonly HwndSource source;
    private readonly Func<bool> isTesting;
    public event Action<Key, bool>? KeyChanged;
    public FocusedKeyboardSource(IntPtr hwnd, Func<bool> isTesting)
    {
        this.isTesting = isTesting;
        source = HwndSource.FromHwnd(hwnd) ?? throw new InvalidOperationException("Window message source unavailable.");
        source.AddHook(OnMessage);
    }

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint code, uint type);
    private IntPtr OnMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (!isTesting() || message is not (0x100 or 0x101 or 0x104 or 0x105))
            return IntPtr.Zero;
        int vk = wParam.ToInt32();
        if (vk == 9)
            return IntPtr.Zero; // Keep Tab/Shift+Tab accessible to leave the tester.
        long bits = lParam.ToInt64();
        bool extended = (bits & (1L << 24)) != 0;
        if (vk == 0x10)
            vk = (int)MapVirtualKey((uint)((bits >> 16) & 0xFF), 3);
        else if (vk == 0x11)
            vk = extended ? 0xA3 : 0xA2;
        else if (vk == 0x12)
            vk = extended ? 0xA5 : 0xA4;
        bool down = message is 0x100 or 0x104;
        if (!down || (bits & (1L << 30)) == 0)
            KeyChanged?.Invoke(KeyInterop.KeyFromVirtualKey(vk), down);
        handled = true;
        return IntPtr.Zero;
    }

    public void Dispose() => source.RemoveHook(OnMessage);
}
