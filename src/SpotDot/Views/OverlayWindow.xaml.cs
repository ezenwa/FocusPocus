using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Canvas = System.Windows.Controls.Canvas;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfPoint = System.Windows.Point;
using SpotDot.Interop;
using SpotDot.Models;
using Forms = System.Windows.Forms;

namespace SpotDot.Views;

public partial class OverlayWindow : Window
{
    private readonly System.Windows.Threading.DispatcherTimer _timer;
    private readonly System.Windows.Threading.DispatcherTimer _keyTimer;
    private AppSettings _settings;
    private bool _spotlightVisible;
    private Forms.Screen _screen;

    public OverlayWindow(AppSettings settings, Forms.Screen screen)
    {
        InitializeComponent();
        _settings = settings;
        _screen = screen;
        _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => UpdateSpot();
        _keyTimer = new() { Interval = TimeSpan.FromSeconds(1.4) };
        _keyTimer.Tick += (_, _) => { _keyTimer.Stop(); KeyBadge.Visibility = Visibility.Collapsed; };
        SourceInitialized += MakeClickThrough;
        ApplySettings(settings);
    }

    private void MakeClickThrough(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE,
            new nint(style | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE));
        MoveToScreen(_screen);
    }

    public void MoveToScreen(Forms.Screen screen, bool fadeIn = false)
    {
        _screen = screen;
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == 0) return;
        if (fadeIn) Opacity = 0.70;
        EnforceScreenBounds();
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
            new Action(EnforceScreenBounds));
        if (fadeIn)
            BeginAnimation(OpacityProperty, new DoubleAnimation(0.70, 1, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
    }

    private void EnforceScreenBounds()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == 0) return;
        var b = _screen.Bounds;
        if (!NativeMethods.GetWindowRect(hwnd, out var current) || current.Left != b.Left || current.Top != b.Top ||
            current.Right - current.Left != b.Width || current.Bottom - current.Top != b.Height)
            NativeMethods.SetWindowPos(hwnd, new nint(-1), b.Left, b.Top, b.Width, b.Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        Dimmer.Fill = CreateSpotBrush(settings);
        KeyLabel.FontSize = settings.KeystrokeFontSize;
    }

    public void SetSpotlight(bool enabled, bool animate = true)
    {
        _spotlightVisible = enabled;
        if (enabled)
        {
            if (!IsVisible) Show();
            _timer.Start();
            if (animate)
            {
                Dimmer.Opacity = 0;
                Dimmer.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            }
            else Dimmer.Opacity = 1;
        }
        else
        {
            _timer.Stop();
            Dimmer.Opacity = 0;
            HideIfIdle();
        }
    }

    private System.Windows.Media.Brush CreateSpotBrush(AppSettings settings)
    {
        var color = (WpfColor)WpfColorConverter.ConvertFromString(settings.OverlayColor);
        color.A = settings.OverlayOpacity;
        var radius = Math.Max(40, settings.SpotDiameter / 2);
        var feather = Math.Clamp(settings.FeatherSize / radius, 0.02, 0.95);
        var brush = new RadialGradientBrush
        {
            MappingMode = BrushMappingMode.Absolute,
            RadiusX = radius,
            RadiusY = radius,
            GradientOrigin = new WpfPoint(0, 0),
            Center = new WpfPoint(0, 0)
        };
        var clear = WpfColor.FromArgb(0, color.R, color.G, color.B);
        var featherStart = Math.Max(0, 1 - feather);
        brush.GradientStops.Add(new GradientStop(clear, 0));
        brush.GradientStops.Add(new GradientStop(clear, featherStart));
        const int samples = 14;
        for (var i = 1; i <= samples; i++)
        {
            var t = i / (double)samples;
            // Quintic smootherstep: zero slope at both ends and no visible shoulder.
            var eased = t * t * t * (t * (t * 6 - 15) + 10);
            var sample = WpfColor.FromArgb((byte)Math.Round(color.A * eased), color.R, color.G, color.B);
            brush.GradientStops.Add(new GradientStop(sample, featherStart + (1 - featherStart) * t));
        }
        return brush;
    }

    private void UpdateSpot()
    {
        if (!_spotlightVisible || !NativeMethods.GetCursorPos(out var point)) return;
        var currentScreen = Forms.Screen.FromPoint(new System.Drawing.Point(point.X, point.Y));
        if (!string.Equals(currentScreen.DeviceName, _screen.DeviceName, StringComparison.OrdinalIgnoreCase))
            MoveToScreen(currentScreen, fadeIn: true);
        EnforceScreenBounds();
        if (Dimmer.Fill is RadialGradientBrush brush)
        {
            var local = PointFromScreen(new WpfPoint(point.X, point.Y));
            brush.Center = local;
            brush.GradientOrigin = local;
        }
    }

    public void ShowClick(bool left, int screenX, int screenY, string color)
    {
        if (!IsVisible) Show();
        var size = Math.Max(80d, _settings.SpotDiameter * 0.88);
        ClickPulse.Width = size;
        ClickPulse.Height = size;
        var pulseColor = (WpfColor)WpfColorConverter.ConvertFromString(color);
        pulseColor.A = 42;
        ClickPulse.Fill = new SolidColorBrush(pulseColor);
        var local = PointFromScreen(new WpfPoint(screenX, screenY));
        Canvas.SetLeft(ClickPulse, local.X - size / 2);
        Canvas.SetTop(ClickPulse, local.Y - size / 2);
        ClickPulse.Visibility = Visibility.Visible;
        var scale = new ScaleTransform(0.92, 0.92, size / 2, size / 2);
        ClickPulse.RenderTransform = scale;
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.92, 1, TimeSpan.FromMilliseconds(220)));
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.92, 1, TimeSpan.FromMilliseconds(220)));
        var fade = new DoubleAnimation(0.55, 0, TimeSpan.FromMilliseconds(220));
        fade.Completed += (_, _) => { ClickPulse.Visibility = Visibility.Collapsed; HideIfIdle(); };
        ClickPulse.BeginAnimation(OpacityProperty, fade);
    }

    public void ShowKey(string text)
    {
        if (!IsVisible) Show();
        KeyLabel.Text = text;
        KeyBadge.Visibility = Visibility.Visible;
        KeyBadge.Opacity = 0;
        KeyBadge.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(110)));
        _keyTimer.Stop();
        _keyTimer.Start();
    }

    private void HideIfIdle()
    {
        if (!_spotlightVisible && ClickPulse.Visibility != Visibility.Visible && KeyBadge.Visibility != Visibility.Visible) Hide();
    }
}
