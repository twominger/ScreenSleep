using System.Drawing.Drawing2D;

namespace ScreenSleep;

internal static class TrayIconFactory
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var monitorBrush = new SolidBrush(Color.FromArgb(76, 70, 210));
        using GraphicsPath monitor = RoundedRectangle(new RectangleF(2, 4, 28, 21), 6);
        graphics.FillPath(monitorBrush, monitor);

        using var screenBrush = new SolidBrush(Color.FromArgb(26, 32, 48));
        using GraphicsPath screen = RoundedRectangle(new RectangleF(5, 7, 22, 15), 3.5f);
        graphics.FillPath(screenBrush, screen);

        using var moonBrush = new SolidBrush(Color.FromArgb(255, 218, 120));
        graphics.FillEllipse(moonBrush, 10, 8, 12, 12);
        graphics.FillEllipse(screenBrush, 14, 6, 11, 11);

        using var standPen = new Pen(Color.FromArgb(76, 70, 210), 2.5f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        graphics.DrawLine(standPen, 16, 24, 16, 28);
        graphics.DrawLine(standPen, 11, 28, 21, 28);

        IntPtr handle = bitmap.GetHicon();
        try
        {
            using Icon temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(handle);
        }
    }

    private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
    {
        float diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
