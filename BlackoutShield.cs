namespace ScreenSleep;

internal sealed class BlackoutShield : IDisposable
{
    private readonly List<BlackoutForm> _windows = [];

    public void Show()
    {
        Hide();

        foreach (Screen screen in Screen.AllScreens)
        {
            var window = new BlackoutForm(screen.Bounds);
            _windows.Add(window);
            window.Show();
        }
    }

    public void Hide()
    {
        foreach (BlackoutForm window in _windows)
        {
            window.Close();
            window.Dispose();
        }
        _windows.Clear();
    }

    public void Dispose() => Hide();

    private sealed class BlackoutForm : Form
    {
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;

        public BlackoutForm(Rectangle bounds)
        {
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Black;
            Bounds = bounds;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= WsExToolWindow | WsExNoActivate;
                return parameters;
            }
        }
    }
}
