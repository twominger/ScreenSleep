namespace ScreenSleep;

internal sealed class ScreenSleepSession : IDisposable
{
    private static readonly TimeSpan InputGuard = TimeSpan.FromMilliseconds(650);

    private readonly InputMonitor _inputMonitor;
    private readonly DisplayPowerMonitor _displayMonitor;
    private readonly BlackoutShield _blackoutShield = new();
    private readonly System.Windows.Forms.Timer _displayTimer;
    private bool _active;
    private bool _displayWasOff;
    private DateTime _activatedAt;

    public event EventHandler<bool>? StateChanged;

    public ScreenSleepSession(InputMonitor inputMonitor, DisplayPowerMonitor displayMonitor)
    {
        _inputMonitor = inputMonitor;
        _displayMonitor = displayMonitor;
        _inputMonitor.KeyPressed += OnKeyPressed;
        _inputMonitor.MouseMoved += OnMouseMoved;
        _inputMonitor.MouseAction += OnMouseAction;
        _displayMonitor.StateChanged += OnDisplayStateChanged;

        _displayTimer = new System.Windows.Forms.Timer();
        _displayTimer.Tick += (_, _) =>
        {
            _displayTimer.Stop();
            if (_active)
                NativeMethods.TurnOffDisplay();
        };
    }

    public void Enter()
    {
        if (!_active)
        {
            _active = true;
            _displayWasOff = false;
            _activatedAt = DateTime.UtcNow;
            _blackoutShield.Show();
            NativeMethods.KeepSystemAwake();
            StateChanged?.Invoke(this, true);
        }

        ScheduleDisplayOff(320);
    }

    private void OnKeyPressed(object? sender, EventArgs e)
    {
        if (_active && DateTime.UtcNow - _activatedAt >= InputGuard)
            ExitScreenSleep();
    }

    private void OnMouseMoved(object? sender, Point point)
    {
        if (_active && DateTime.UtcNow - _activatedAt >= InputGuard)
            ExitScreenSleep();
    }

    private void OnMouseAction(object? sender, EventArgs e)
    {
        if (_active && DateTime.UtcNow - _activatedAt >= InputGuard)
            ExitScreenSleep();
    }

    private void OnDisplayStateChanged(object? sender, DisplayState state)
    {
        if (!_active)
            return;

        if (state == DisplayState.Off)
        {
            _displayWasOff = true;
            return;
        }

        if (state == DisplayState.On && _displayWasOff)
            ScheduleDisplayOff(220);
    }

    private void ScheduleDisplayOff(int milliseconds)
    {
        _displayTimer.Stop();
        _displayTimer.Interval = milliseconds;
        _displayTimer.Start();
    }

    private void ExitScreenSleep()
    {
        if (!_active)
            return;

        _active = false;
        _displayTimer.Stop();
        _blackoutShield.Hide();
        NativeMethods.AllowSystemSleep();
        NativeMethods.WakeDisplay();
        StateChanged?.Invoke(this, false);
    }

    public void Dispose()
    {
        if (_active)
            ExitScreenSleep();
        _displayTimer.Dispose();
        _blackoutShield.Dispose();
        _inputMonitor.KeyPressed -= OnKeyPressed;
        _inputMonitor.MouseMoved -= OnMouseMoved;
        _inputMonitor.MouseAction -= OnMouseAction;
        _displayMonitor.StateChanged -= OnDisplayStateChanged;
    }
}
