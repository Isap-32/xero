using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using KeyBoost.Core.Models;

namespace KeyBoost.Native;
internal static class KeyboardNative
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, ref uint value, uint flags);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, IntPtr value, uint flags);
    public static KeyboardSettings Read()
    {
        uint delay = 0, speed = 0;
        if (!SystemParametersInfo(0x16, 0, ref delay, 0) || !SystemParametersInfo(0x0A, 0, ref speed, 0))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return new((int)delay, (int)speed);
    }

    public static void Apply(KeyboardSettings settings)
    {
        settings.Validate();
        if (!SystemParametersInfo(0x17, (uint)settings.Delay, IntPtr.Zero, 3))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (!SystemParametersInfo(0x0B, (uint)settings.Speed, IntPtr.Zero, 3))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (Read() != settings)
            throw new InvalidOperationException("Windows read-back differs from the requested settings. Restore previous settings if needed.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Device
    {
        public IntPtr Handle;
        public uint Type;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputDeviceList([Out] Device[]? devices, ref uint count, uint size);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, System.Text.StringBuilder? data, ref uint size);
    public static string Devices()
    {
        uint count = 0;
        uint size = (uint)Marshal.SizeOf<Device>();
        if (GetRawInputDeviceList(null, ref count, size) == uint.MaxValue)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        var devices = new Device[count];
        if (GetRawInputDeviceList(devices, ref count, size) == uint.MaxValue)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        var names = new System.Collections.Generic.List<string>();
        for (int i = 0; i < count; i++)
            if (devices[i].Type == 1)
            {
                uint length = 0;
                if (GetRawInputDeviceInfo(devices[i].Handle, 0x20000007, null, ref length) == uint.MaxValue)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                var name = new System.Text.StringBuilder((int)length + 1);
                if (GetRawInputDeviceInfo(devices[i].Handle, 0x20000007, name, ref length) == uint.MaxValue)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                names.Add(name.ToString());
            }

        return names.Count == 0 ? "No keyboard devices exposed by Windows." : string.Join(Environment.NewLine, names);
    }
}
