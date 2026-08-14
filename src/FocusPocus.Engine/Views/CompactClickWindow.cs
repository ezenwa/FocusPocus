using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FocusPocus.Engine.Interop;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;

namespace FocusPocus.Engine.Views;

public sealed class CompactClickWindow : Window
{
    private const double WindowSize = 64;
    private const double PulseSize = 36;
    private readonly Ellipse _pulse;
    private int _pulseGeneration;

    public CompactClickWindow()
    {
        Width = WindowSize;
        Height = WindowSize;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;

        var canvas = new Canvas { Width = WindowSize, Height = WindowSize, IsHitTestVisible = false };
        _pulse = new Ellipse { Width = PulseSize, Height = PulseSize, StrokeThickness = 3 };
        Canvas.SetLeft(_pulse, (WindowSize - PulseSize) / 2);
        Canvas.SetTop(_pulse, (WindowSize - PulseSize) / 2);
        canvas.Children.Add(_pulse);
        Content = canvas;
        SourceInitialized += MakeClickThrough;
    }

    private void MakeClickThrough(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE,
            new nint(style | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE));
    }

    public void ShowPulse(int screenX, int screenY, string color)
    {
        var generation = ++_pulseGeneration;
        if (!IsVisible)
        {
            Opacity = 0;
            Show();
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var widthPixels = (int)Math.Ceiling(WindowSize * dpi.DpiScaleX);
        var heightPixels = (int)Math.Ceiling(WindowSize * dpi.DpiScaleY);
        var hwnd = new WindowInteropHelper(this).Handle;
        NativeMethods.SetWindowPos(hwnd, new nint(-1), screenX - widthPixels / 2, screenY - heightPixels / 2,
            widthPixels, heightPixels, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);

        var pulseColor = (WpfColor)WpfColorConverter.ConvertFromString(color);
        var fillColor = pulseColor;
        fillColor.A = 26;
        pulseColor.A = 230;
        _pulse.Fill = new SolidColorBrush(fillColor);
        _pulse.Stroke = new SolidColorBrush(pulseColor);
        _pulse.Visibility = Visibility.Visible;
        _pulse.BeginAnimation(OpacityProperty, null);

        const double startScale = 0.55;
        const double endScale = 1.35;
        var duration = TimeSpan.FromMilliseconds(260);
        var scale = new ScaleTransform(startScale, startScale, PulseSize / 2, PulseSize / 2);
        _pulse.RenderTransform = scale;
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(startScale, endScale, duration));
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(startScale, endScale, duration));
        var fade = new DoubleAnimation(1, 0, duration);
        fade.Completed += (_, _) =>
        {
            if (generation == _pulseGeneration) Hide();
        };
        _pulse.BeginAnimation(OpacityProperty, fade);
        Opacity = 1;
    }
}
