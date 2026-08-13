using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace FocusPocus;

public partial class App : Application
{
    private Window? _window;
    private Mutex? _mutex;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            try
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusPocus");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "winui-crash.log"), e.Exception.ToString());
            }
            catch { }
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mutex = new Mutex(true, "FocusPocus.WinUI.Singleton", out var created);
        if (!created)
        {
            var existing = Process.GetProcessesByName("FocusPocus").FirstOrDefault(p => p.Id != Environment.ProcessId);
            if (existing?.MainWindowHandle is { } hwnd && hwnd != IntPtr.Zero) SetForegroundWindow(hwnd);
            Exit();
            return;
        }

        EnsureEngineRunning();
        _window = new MainWindow();
        _window.Activate();
    }

    private static void EnsureEngineRunning()
    {
        if (Process.GetProcessesByName("FocusPocus.Engine").Length > 0) return;
        var engine = Path.Combine(AppContext.BaseDirectory, "FocusPocus.Engine.exe");
        if (File.Exists(engine)) Process.Start(new ProcessStartInfo(engine, "--tray") { UseShellExecute = true });
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hwnd);
}
