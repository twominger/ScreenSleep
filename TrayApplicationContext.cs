namespace ScreenSleep;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _sleepItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly InputMonitor _inputMonitor;
    private readonly DisplayPowerMonitor _displayMonitor;
    private readonly ScreenSleepSession _session;
    private readonly Icon _icon;

    public TrayApplicationContext()
    {
        _icon = TrayIconFactory.Create();
        _sleepItem = new ToolStripMenuItem("立即息屏", null, (_, _) => EnterScreenSleep());
        _startupItem = new ToolStripMenuItem("开机自动启动")
        {
            CheckOnClick = true,
            Checked = StartupManager.IsEnabled
        };
        _startupItem.CheckedChanged += StartupItem_CheckedChanged;

        var menu = new ContextMenuStrip();
        menu.Items.Add(_sleepItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripMenuItem("关于 ScreenSleep", null, ShowAbout));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("退出", null, (_, _) => ExitApplication()));

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "ScreenSleep - 点击进入息屏模式",
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        _inputMonitor = new InputMonitor();
        _displayMonitor = new DisplayPowerMonitor();
        _session = new ScreenSleepSession(_inputMonitor, _displayMonitor);
        _session.StateChanged += Session_StateChanged;
        _inputMonitor.Start();

        _notifyIcon.ShowBalloonTip(
            2500,
            "ScreenSleep 已运行",
            "左键点击托盘图标即可关闭显示器。",
            ToolTipIcon.Info);
    }

    private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            EnterScreenSleep();
    }

    private void EnterScreenSleep()
    {
        try
        {
            _session.Enter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"无法进入息屏模式：{ex.Message}",
                "ScreenSleep",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void Session_StateChanged(object? sender, bool isActive)
    {
        _sleepItem.Text = isActive ? "重新关闭显示器" : "立即息屏";
        _notifyIcon.Text = isActive
            ? "ScreenSleep - 息屏模式中"
            : "ScreenSleep - 点击进入息屏模式";
    }

    private void StartupItem_CheckedChanged(object? sender, EventArgs e)
    {
        try
        {
            StartupManager.SetEnabled(_startupItem.Checked);
        }
        catch (Exception ex)
        {
            _startupItem.CheckedChanged -= StartupItem_CheckedChanged;
            _startupItem.Checked = StartupManager.IsEnabled;
            _startupItem.CheckedChanged += StartupItem_CheckedChanged;
            MessageBox.Show(
                $"无法修改开机启动：{ex.Message}",
                "ScreenSleep",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static void ShowAbout(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "ScreenSleep 1.0.3\n\n左键点击托盘图标进入息屏模式。\n按正常工作的键盘、移动或点击鼠标即可恢复。\n\n已禁用的内置键盘不会解除息屏。",
            "关于 ScreenSleep",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ExitApplication()
    {
        _notifyIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _session.Dispose();
            _inputMonitor.Dispose();
            _displayMonitor.Dispose();
            _notifyIcon.Dispose();
            _icon.Dispose();
        }
        base.Dispose(disposing);
    }
}
