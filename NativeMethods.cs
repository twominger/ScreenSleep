using System.Runtime.InteropServices;

namespace ScreenSleep;

internal static class NativeMethods
{
    private static readonly IntPtr HwndBroadcast = new(0xFFFF);
    private const uint WmSysCommand = 0x0112;
    private static readonly IntPtr ScMonitorPower = new(0xF170);
    private const uint SmtoAbortIfHung = 0x0002;
    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private const uint EsDisplayRequired = 0x00000002;

    internal delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseHookData
    {
        public NativePoint Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PowerBroadcastSetting
    {
        public Guid PowerSetting;
        public uint DataLength;
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr window,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        uint flags,
        uint timeout,
        out IntPtr result);

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint executionState);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr RegisterPowerSettingNotification(
        IntPtr recipient,
        ref Guid powerSettingGuid,
        uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterPowerSettingNotification(IntPtr handle);

    internal static void TurnOffDisplay()
        => SendMonitorPowerCommand(new IntPtr(2));

    internal static void WakeDisplay()
    {
        SetThreadExecutionState(EsContinuous | EsSystemRequired | EsDisplayRequired);
        SendMonitorPowerCommand(new IntPtr(-1));
        SetThreadExecutionState(EsContinuous);
    }

    private static void SendMonitorPowerCommand(IntPtr state)
    {
        SendMessageTimeout(
            HwndBroadcast,
            WmSysCommand,
            ScMonitorPower,
            state,
            SmtoAbortIfHung,
            1000,
            out _);
    }

    internal static void KeepSystemAwake()
        => SetThreadExecutionState(EsContinuous | EsSystemRequired);

    internal static void AllowSystemSleep()
        => SetThreadExecutionState(EsContinuous);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool DestroyIcon(IntPtr icon);
}
