using System.Windows;
using FocusPocus.Engine.Services;

namespace FocusPocus.Engine;

public partial class App : System.Windows.Application
{
    private AppController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _controller = new AppController();
        _controller.Start(e.Args.Contains("--tray", StringComparer.OrdinalIgnoreCase));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _controller?.Dispose();
        base.OnExit(e);
    }
}
