using System.Runtime.InteropServices;

namespace ScreenSleep;

internal sealed class InputMonitor : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WhMouseLl = 14;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int WmMouseMove = 0x0200;

    private readonly NativeMethods.HookProc _keyboardCallback;
    private readonly NativeMethods.HookProc _mouseCallback;
    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;

    public event EventHandler? KeyPressed;
    public event EventHandler<Point>? MouseMoved;
    public event EventHandler? MouseAction;

    public InputMonitor()
    {
        _keyboardCallback = KeyboardHookCallback;
        _mouseCallback = MouseHookCallback;
    }

    public void Start()
    {
        IntPtr module = NativeMethods.GetModuleHandle(null);
        _keyboardHook = NativeMethods.SetWindowsHookEx(WhKeyboardLl, _keyboardCallback, module, 0);
        _mouseHook = NativeMethods.SetWindowsHookEx(WhMouseLl, _mouseCallback, module, 0);

        if (_keyboardHook == IntPtr.Zero || _mouseHook == IntPtr.Zero)
        {
            Dispose();
            throw new InvalidOperationException("无法安装全局输入监听器。");
        }
    }

    private IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && (wParam.ToInt32() == WmKeyDown || wParam.ToInt32() == WmSysKeyDown))
            KeyPressed?.Invoke(this, EventArgs.Empty);

        return NativeMethods.CallNextHookEx(_keyboardHook, code, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            if (wParam.ToInt32() == WmMouseMove)
            {
                var data = Marshal.PtrToStructure<NativeMethods.MouseHookData>(lParam);
                MouseMoved?.Invoke(this, new Point(data.Point.X, data.Point.Y));
            }
            else
            {
                MouseAction?.Invoke(this, EventArgs.Empty);
            }
        }

        return NativeMethods.CallNextHookEx(_mouseHook, code, wParam, lParam);
    }

    public void Dispose()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }

        if (_mouseHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }
}
