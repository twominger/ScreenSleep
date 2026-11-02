using System.Threading;

namespace ScreenSleep;

internal static class Program
{
    private const string MutexName = "ScreenSleep.SingleInstance.A751C32B";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, MutexName, out bool isFirstInstance);
        if (!isFirstInstance)
            return;

        ApplicationConfiguration.Initialize();
        using var context = new TrayApplicationContext();
        Application.Run(context);
    }
}
