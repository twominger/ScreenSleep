using System.Runtime.InteropServices;

namespace ScreenSleep;

internal sealed class DisplayPowerMonitor : NativeWindow, IDisposable
{
    private const int WmPowerBroadcast = 0x0218;
    private const int PbtPowerSettingChange = 0x8013;
    private static readonly Guid SessionDisplayStatus = new("2B84C20E-AD23-4DDF-93DB-05FFBD7EFCA5");

    private IntPtr _registration;

    public event EventHandler<DisplayState>? StateChanged;

    public DisplayPowerMonitor()
    {
        CreateHandle(new CreateParams
        {
            Caption = "ScreenSleep.PowerEvents"
        });

        Guid setting = SessionDisplayStatus;
        _registration = NativeMethods.RegisterPowerSettingNotification(Handle, ref setting, 0);
        if (_registration == IntPtr.Zero)
        {
            DestroyHandle();
            throw new InvalidOperationException("无法监听显示器电源状态。");
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == WmPowerBroadcast && message.WParam.ToInt32() == PbtPowerSettingChange)
        {
            var setting = Marshal.PtrToStructure<NativeMethods.PowerBroadcastSetting>(message.LParam);
            if (setting.PowerSetting == SessionDisplayStatus && setting.DataLength >= sizeof(int))
            {
                int dataOffset = Marshal.SizeOf<NativeMethods.PowerBroadcastSetting>();
                int value = Marshal.ReadInt32(message.LParam, dataOffset);
                if (Enum.IsDefined(typeof(DisplayState), value))
                    StateChanged?.Invoke(this, (DisplayState)value);
            }
        }

        base.WndProc(ref message);
    }

    public void Dispose()
    {
        if (_registration != IntPtr.Zero)
        {
            NativeMethods.UnregisterPowerSettingNotification(_registration);
            _registration = IntPtr.Zero;
        }

        if (Handle != IntPtr.Zero)
            DestroyHandle();
    }
}

internal enum DisplayState
{
    Off = 0,
    On = 1,
    Dimmed = 2
}
