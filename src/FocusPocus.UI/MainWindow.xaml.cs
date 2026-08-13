using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace FocusPocus;

public sealed partial class MainWindow : Window
{
    private readonly SettingsService _storage = new();
    private AppSettings _settings;
    private bool _loading = true;
    private NavigationViewItem? _currentNav;
    private AppWindow? _appWindow;

    private readonly Grid RootGrid = new();
    private readonly Grid AppTitleBar = new();
    private readonly Border TitleLogo = new(), BrandLogo = new(), ColorPreview = new();
    private readonly NavigationView NavView = new();
    private readonly NavigationViewItem SpotNav = new() { Tag = "spot" }, EffectsNav = new() { Tag = "effects" }, ShortcutsNav = new() { Tag = "shortcuts" }, BehaviorNav = new() { Tag = "behavior" }, AboutNav = new() { Tag = "about" };
    private readonly TextBlock PageTitle = new(), SubtitleText = new(), BrandSlogan = new(), SpotEnabledHeader = new(), SizeLabel = new(), SizeValue = new(), FeatherLabel = new(), FeatherValue = new(), OverlayTitle = new(), ColorLabel = new(), OpacityLabel = new(), OpacityValue = new(), EffectsTitle = new(), ClicksLabel = new(), KeysLabel = new(), OnlyShortcutsLabel = new(), SoundLabel = new(), KeySizeLabel = new(), KeySizeValue = new(), ShortcutsTitle = new(), SpotShortcutLabel = new(), ClickShortcutLabel = new(), KeyShortcutLabel = new(), IncreaseSpotLabel = new(), DecreaseSpotLabel = new(), DecreaseOpacityLabel = new(), IncreaseOpacityLabel = new(), BehaviorTitle = new(), StartupLabel = new(), TrayLabel = new();
    private readonly ComboBox LanguageBox = new();
    private readonly StackPanel SpotPage = new(), EffectsPage = new(), ShortcutsPage = new(), BehaviorPage = new();
    private readonly ToggleSwitch SpotEnabled = new(), ClicksEnabled = new(), KeysEnabled = new(), ShortcutsOnly = new(), SoundEnabled = new(), StartupEnabled = new(), TrayEnabled = new();
    private readonly Slider SizeSlider = new() { Minimum = 100, Maximum = 800, StepFrequency = 10 }, FeatherSlider = new() { Minimum = 10, Maximum = 220, StepFrequency = 5 }, KeySizeSlider = new() { Minimum = 14, Maximum = 48, StepFrequency = 1 }, OpacitySlider = new() { Minimum = 0, Maximum = 100, StepFrequency = 1 };
    private readonly TextBox ColorBox = new(), SpotShortcut = new(), ClickShortcut = new(), KeyShortcut = new(), IncreaseSpotShortcut = new(), DecreaseSpotShortcut = new(), DecreaseOpacityShortcut = new(), IncreaseOpacityShortcut = new();
    private readonly Button ChooseColorButton = new(), ApplyButton = new() { MinWidth = 112 };
    private readonly ColorPicker OverlayColorPicker = new() { IsAlphaEnabled = false, IsAlphaSliderVisible = false, IsAlphaTextInputVisible = false };
    private readonly InfoBar PrivacyBar = new() { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Informational }, StartupInfo = new() { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning };

    public MainWindow()
    {
        InitializeComponent();
        BuildInterface();
        var logo = new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "FocusPocus.png"));
        TitleLogo.Background = new ImageBrush { ImageSource = new BitmapImage(logo), Stretch = Stretch.Uniform };
        BrandLogo.Background = new ImageBrush { ImageSource = new BitmapImage(logo), Stretch = Stretch.Uniform };
        _settings = _storage.Load();
        ConfigureWindow();
        LoadValues();
        _currentNav = SpotNav;
    }

    private void BuildInterface()
    {
        var background = new SolidColorBrush(ColorHelper.FromArgb(255, 22, 11, 36));
        var muted = new SolidColorBrush(ColorHelper.FromArgb(255, 216, 203, 231));
        var accent = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 221, 85));
        RootGrid.Background = background;
        RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
        RootGrid.RowDefinitions.Add(new RowDefinition());
        XamlRootHost.Children.Add(RootGrid);

        AppTitleBar.Padding = new Thickness(16, 0, 16, 0);
        AppTitleBar.Background = new SolidColorBrush(ColorHelper.FromArgb(179, 22, 11, 36));
        var title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        TitleLogo.Width = TitleLogo.Height = 22;
        title.Children.Add(TitleLogo);
        title.Children.Add(new TextBlock { Text = "FocusPocus", FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        AppTitleBar.Children.Add(title);
        RootGrid.Children.Add(AppTitleBar);

        Grid.SetRow(NavView, 1);
        NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
        NavView.IsPaneOpen = true;
        NavView.IsPaneToggleButtonVisible = false;
        NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        NavView.IsSettingsVisible = false;
        NavView.OpenPaneLength = 260;
        NavView.CompactPaneLength = 52;
        NavView.Background = new SolidColorBrush(Colors.Transparent);
        BrandLogo.Width = BrandLogo.Height = 48;
        var brandBlock = new StackPanel { Margin = new Thickness(8, 12, 8, 34) };
        var brand = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        brand.Children.Add(BrandLogo);
        var brandText = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        brandText.Children.Add(new TextBlock { Text = "FocusPocus", FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        brandText.Children.Add(new TextBlock { Text = "2.1.0", FontSize = 12, Foreground = muted });
        brand.Children.Add(brandText);
        brandBlock.Children.Add(brand);
        BrandSlogan.Margin = new Thickness(0, 10, 0, 0);
        BrandSlogan.FontSize = 13;
        BrandSlogan.Foreground = muted;
        BrandSlogan.TextWrapping = TextWrapping.Wrap;
        brandBlock.Children.Add(BrandSlogan);
        NavView.PaneHeader = brandBlock;
        SetNavIcon(SpotNav, "\uE774"); SetNavIcon(EffectsNav, "\uE7C9"); SetNavIcon(ShortcutsNav, "\uE765"); SetNavIcon(BehaviorNav, "\uE713"); SetNavIcon(AboutNav, "\uE946");
        NavView.MenuItems.Add(SpotNav); NavView.MenuItems.Add(EffectsNav); NavView.MenuItems.Add(ShortcutsNav); NavView.MenuItems.Add(BehaviorNav); NavView.FooterMenuItems.Add(AboutNav);
        RootGrid.Children.Add(NavView);

        var content = new Grid { Margin = new Thickness(38, 18, 38, 20) };
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition());
        var header = new Grid();
        var heading = new StackPanel();
        PageTitle.FontSize = 34; PageTitle.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; PageTitle.Margin = new Thickness(0, 0, 0, 18);
        SubtitleText.Visibility = Visibility.Collapsed;
        heading.Children.Add(PageTitle); header.Children.Add(heading);
        LanguageBox.Width = 138; LanguageBox.HorizontalAlignment = HorizontalAlignment.Right; LanguageBox.VerticalAlignment = VerticalAlignment.Top;
        LanguageBox.Items.Add(new ComboBoxItem { Tag = "es", Content = "Español" }); LanguageBox.Items.Add(new ComboBoxItem { Tag = "en", Content = "English" });
        header.Children.Add(LanguageBox); content.Children.Add(header);

        var pages = new Grid(); Grid.SetRow(pages, 1); content.Children.Add(pages);
        BuildSpotPage(accent, muted); BuildEffectsPage(accent, muted); BuildShortcutsPage(accent); BuildBehaviorPage(accent);
        EffectsPage.Visibility = ShortcutsPage.Visibility = BehaviorPage.Visibility = Visibility.Collapsed;
        pages.Children.Add(SpotPage); pages.Children.Add(EffectsPage); pages.Children.Add(ShortcutsPage); pages.Children.Add(BehaviorPage);
        NavView.Content = content;

        NavView.SelectionChanged += NavigationChanged; LanguageBox.SelectionChanged += LanguageChanged;
        foreach (var toggle in new[] { SpotEnabled, ClicksEnabled, KeysEnabled, ShortcutsOnly, SoundEnabled, StartupEnabled, TrayEnabled }) toggle.Toggled += LiveChanged;
        foreach (var slider in new[] { SizeSlider, FeatherSlider, KeySizeSlider, OpacitySlider }) slider.ValueChanged += LiveChanged;
        OverlayColorPicker.ColorChanged += ColorPickerChanged;
        foreach (var box in new[] { SpotShortcut, ClickShortcut, KeyShortcut, IncreaseSpotShortcut, DecreaseSpotShortcut, DecreaseOpacityShortcut, IncreaseOpacityShortcut }) box.KeyDown += CaptureShortcut;
        SpotNav.IsSelected = true;
    }

    private static void SetNavIcon(NavigationViewItem item, string glyph) => item.Icon = new FontIcon { Glyph = glyph };
    private static Border Card(UIElement child) => new() { Child = child, Background = new SolidColorBrush(ColorHelper.FromArgb(230, 45, 21, 73)), BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(128, 98, 64, 138)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Padding = new Thickness(20), Margin = new Thickness(0, 0, 0, 12) };
    private static void StyleHeading(TextBlock text, Brush accent) { text.FontSize = 20; text.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; text.Foreground = accent; text.Margin = new Thickness(0, 0, 0, 8); }
    private static Grid ValueRow(TextBlock label, TextBlock value, Brush muted, double top = 14) { var row = new Grid { Margin = new Thickness(0, top, 0, 4) }; value.HorizontalAlignment = HorizontalAlignment.Right; value.Foreground = muted; row.Children.Add(label); row.Children.Add(value); return row; }
    private static Grid ToggleRow(TextBlock label, ToggleSwitch toggle, double indent = 0) { var row = new Grid { MinHeight = 48, Margin = new Thickness(indent, 0, 0, 0) }; label.VerticalAlignment = VerticalAlignment.Center; toggle.HorizontalAlignment = HorizontalAlignment.Right; toggle.VerticalAlignment = VerticalAlignment.Center; row.Children.Add(label); row.Children.Add(toggle); return row; }

    private void BuildSpotPage(Brush accent, Brush muted)
    {
        var focus = new StackPanel(); SpotEnabledHeader.Foreground = accent; SpotEnabledHeader.FontSize = 20; SpotEnabledHeader.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; SpotEnabled.Header = SpotEnabledHeader; SpotEnabled.FontSize = 17; SpotEnabled.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; SpotEnabled.Foreground = new SolidColorBrush(Colors.White); focus.Children.Add(SpotEnabled); focus.Children.Add(ValueRow(SizeLabel, SizeValue, muted, 8)); focus.Children.Add(SizeSlider); focus.Children.Add(ValueRow(FeatherLabel, FeatherValue, muted, 8)); focus.Children.Add(FeatherSlider); SpotPage.Children.Add(Card(focus));
        var overlay = new StackPanel(); StyleHeading(OverlayTitle, accent); overlay.Children.Add(OverlayTitle);
        var columns = new Grid { ColumnSpacing = 28 }; columns.ColumnDefinitions.Add(new ColumnDefinition()); columns.ColumnDefinitions.Add(new ColumnDefinition());
        var color = new StackPanel();
        var colorRow = new Grid { Margin = new Thickness(0, 9, 0, 0), ColumnSpacing = 10 }; colorRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) }); colorRow.ColumnDefinitions.Add(new ColumnDefinition()); ColorPreview.Width = ColorPreview.Height = 34; ColorPreview.CornerRadius = new CornerRadius(7); ColorPreview.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(136, 255, 255, 255)); ColorPreview.BorderThickness = new Thickness(1); colorRow.Children.Add(ColorPreview); Grid.SetColumn(ChooseColorButton, 1); ChooseColorButton.HorizontalAlignment = HorizontalAlignment.Stretch; ChooseColorButton.Flyout = new Flyout { Content = OverlayColorPicker }; colorRow.Children.Add(ChooseColorButton); color.Children.Add(colorRow); columns.Children.Add(color);
        var opacity = new StackPanel(); opacity.Children.Add(ValueRow(OpacityLabel, OpacityValue, muted, 12)); opacity.Children.Add(OpacitySlider); Grid.SetColumn(opacity, 1); columns.Children.Add(opacity); overlay.Children.Add(columns); SpotPage.Children.Add(Card(overlay));
    }

    private void BuildEffectsPage(Brush accent, Brush muted)
    {
        var panel = new StackPanel(); StyleHeading(EffectsTitle, accent); panel.Children.Add(EffectsTitle);
        panel.Children.Add(ToggleRow(ClicksLabel, ClicksEnabled)); panel.Children.Add(ToggleRow(KeysLabel, KeysEnabled)); panel.Children.Add(ToggleRow(OnlyShortcutsLabel, ShortcutsOnly, 24)); panel.Children.Add(ToggleRow(SoundLabel, SoundEnabled)); panel.Children.Add(ValueRow(KeySizeLabel, KeySizeValue, muted, 8)); panel.Children.Add(KeySizeSlider); PrivacyBar.Margin = new Thickness(0, 10, 0, 0); panel.Children.Add(PrivacyBar); EffectsPage.Children.Add(Card(panel));
    }

    private void BuildShortcutsPage(Brush accent)
    {
        var panel = new StackPanel(); StyleHeading(ShortcutsTitle, accent); panel.Children.Add(ShortcutsTitle);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0), ColumnSpacing = 24, RowSpacing = 10 }; grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) }); grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var i = 0; i < 7; i++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var labels = new[] { SpotShortcutLabel, ClickShortcutLabel, KeyShortcutLabel, IncreaseSpotLabel, DecreaseSpotLabel, DecreaseOpacityLabel, IncreaseOpacityLabel }; var boxes = new[] { SpotShortcut, ClickShortcut, KeyShortcut, IncreaseSpotShortcut, DecreaseSpotShortcut, DecreaseOpacityShortcut, IncreaseOpacityShortcut };
        for (var i = 0; i < labels.Length; i++) { labels[i].VerticalAlignment = VerticalAlignment.Center; Grid.SetRow(labels[i], i); grid.Children.Add(labels[i]); Grid.SetRow(boxes[i], i); Grid.SetColumn(boxes[i], 1); grid.Children.Add(boxes[i]); }
        panel.Children.Add(grid); ShortcutsPage.Children.Add(Card(panel));
    }

    private void BuildBehaviorPage(Brush accent)
    {
        var panel = new StackPanel(); StyleHeading(BehaviorTitle, accent); panel.Children.Add(BehaviorTitle); panel.Children.Add(ToggleRow(StartupLabel, StartupEnabled)); panel.Children.Add(ToggleRow(TrayLabel, TrayEnabled)); StartupInfo.Margin = new Thickness(0, 10, 0, 0); panel.Children.Add(StartupInfo); BehaviorPage.Children.Add(Card(panel));
    }

    private void ConfigureWindow()
    {
        Title = "FocusPocus";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        try { SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt }; } catch { }

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var id = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(id);
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
        _appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "FocusPocus.ico"));
        var area = DisplayArea.Primary.WorkArea;
        var width = Math.Clamp((int)Math.Round(area.Width * 0.90), 960, 1280);
        var height = Math.Clamp((int)Math.Round(area.Height * 0.92), 720, 960);
        width = Math.Min(width, area.Width);
        height = Math.Min(height, area.Height);
        _appWindow.MoveAndResize(new RectInt32(area.X + Math.Max(0, (area.Width - width) / 2), area.Y + Math.Max(0, (area.Height - height) / 2), width, height));
        _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }

    private void LoadValues()
    {
        _loading = true;
        SpotEnabled.IsOn = _settings.SpotlightEnabled;
        ClicksEnabled.IsOn = _settings.ClicksEnabled;
        KeysEnabled.IsOn = _settings.KeystrokesEnabled;
        ShortcutsOnly.IsOn = _settings.ShowShortcutsOnly;
        SoundEnabled.IsOn = _settings.ClickSoundEnabled;
        StartupEnabled.IsOn = _settings.StartWithWindows;
        TrayEnabled.IsOn = _settings.MinimizeToTray;
        SizeSlider.Value = _settings.SpotDiameter;
        FeatherSlider.Value = _settings.FeatherSize;
        KeySizeSlider.Value = _settings.KeystrokeFontSize;
        OpacitySlider.Value = _settings.OverlayOpacityPercent;
        ColorBox.Text = _settings.OverlayColor;
        SpotShortcut.Text = _settings.SpotlightHotkey;
        ClickShortcut.Text = _settings.ClicksHotkey;
        KeyShortcut.Text = _settings.KeystrokesHotkey;
        IncreaseSpotShortcut.Text = _settings.IncreaseSpotSizeHotkey;
        DecreaseSpotShortcut.Text = _settings.DecreaseSpotSizeHotkey;
        DecreaseOpacityShortcut.Text = _settings.DecreaseOverlayOpacityHotkey;
        IncreaseOpacityShortcut.Text = _settings.IncreaseOverlayOpacityHotkey;
        LanguageBox.SelectedIndex = _settings.Language == "en" ? 1 : 0;
        UpdateColorPreview();
        UpdateValues();
        Translate();
        _loading = false;
    }

    private string T(string key) => (key, _settings.Language) switch
    {
        ("Subtitle", "en") => "Magic that follows every move",
        ("Focus", "en") => "Spotlight",
        ("EnableFocus", "en") => "Enable spotlight",
        ("FocusSize", "en") => "Spot size",
        ("Feather", "en") => "Edge feather",
        ("Overlay", "en") => "Overlay",
        ("Color", "en") => "Overlay color",
        ("ChooseColor", "en") => "Choose color…",
        ("Opacity", "en") => "Opacity",
        ("Effects", "en") => "Input effects",
        ("Clicks", "en") => "Show mouse clicks",
        ("Keys", "en") => "Show keystrokes",
        ("OnlyShortcuts", "en") => "Only show shortcuts",
        ("Sound", "en") => "Play a sound on click",
        ("KeySize", "en") => "Keystroke size",
        ("Privacy", "en") => "Keystrokes are hidden in password fields.",
        ("Shortcuts", "en") => "Global shortcuts",
        ("ToggleFocus", "en") => "Toggle spotlight",
        ("ToggleClicks", "en") => "Toggle clicks",
        ("ToggleKeys", "en") => "Toggle keystrokes",
        ("IncreaseFocus", "en") => "Increase spotlight size",
        ("DecreaseFocus", "en") => "Decrease spotlight size",
        ("DecreaseOpacity", "en") => "Decrease overlay opacity",
        ("IncreaseOpacity", "en") => "Increase overlay opacity",
        ("Behavior", "en") => "Behavior",
        ("Startup", "en") => "Start with Windows",
        ("Tray", "en") => "Keep running in the system tray",
        ("StartupInfo", "en") => "Windows startup launches the engine minimized in the system tray. The settings window stays closed.",
        ("Apply", "en") => "Apply",
        ("About", "en") => "About",
        ("InvalidColor", "en") => "Enter a valid hexadecimal color in #RRGGBB format.",
        ("InvalidShortcuts", "en") => "All shortcuts must be valid and different.",
        ("Close", "en") => "Close",
        ("Activated", "en") => "Activated",
        ("Deactivated", "en") => "Deactivated",
        ("CheckUpdates", "en") => "Check for updates",
        ("CheckingUpdates", "en") => "Checking for updates…",
        ("UpToDate", "en") => "You're using the latest version of FocusPocus.",
        ("UpdateAvailable", "en") => "A new FocusPocus version is available.",
        ("OpenRelease", "en") => "View and download",
        ("UpdateError", "en") => "FocusPocus couldn't check for updates. Check your internet connection and try again.",
        ("Version", "en") => "Version",
        ("Author", "en") => "Author",
        ("Subtitle", _) => "Magia que sigue tus movimientos",
        ("Focus", _) => "Foco",
        ("EnableFocus", _) => "Activar foco",
        ("FocusSize", _) => "Tamaño del foco",
        ("Feather", _) => "Difuminado del contorno",
        ("Overlay", _) => "Overlay",
        ("Color", _) => "Color del overlay",
        ("ChooseColor", _) => "Elegir color…",
        ("Opacity", _) => "Opacidad",
        ("Effects", _) => "Efectos de entrada",
        ("Clicks", _) => "Mostrar clics",
        ("Keys", _) => "Mostrar teclas pulsadas",
        ("OnlyShortcuts", _) => "Solo mostrar atajos",
        ("Sound", _) => "Reproducir sonido al hacer clic",
        ("KeySize", _) => "Tamaño de las teclas",
        ("Privacy", _) => "Las teclas se ocultan en campos de contraseña.",
        ("Shortcuts", _) => "Atajos globales",
        ("ToggleFocus", _) => "Alternar foco",
        ("ToggleClicks", _) => "Alternar clics",
        ("ToggleKeys", _) => "Alternar teclas",
        ("IncreaseFocus", _) => "Aumentar foco",
        ("DecreaseFocus", _) => "Disminuir foco",
        ("DecreaseOpacity", _) => "Reducir opacidad",
        ("IncreaseOpacity", _) => "Aumentar opacidad",
        ("Behavior", _) => "Comportamiento",
        ("Startup", _) => "Iniciar con Windows",
        ("Tray", _) => "Mantener en la bandeja del sistema",
        ("StartupInfo", _) => "El inicio con Windows ejecuta el motor minimizado en la bandeja. La ventana de configuración permanece cerrada.",
        ("Apply", _) => "Aplicar",
        ("About", _) => "Acerca de",
        ("InvalidColor", _) => "Introduce un color hexadecimal válido con formato #RRGGBB.",
        ("InvalidShortcuts", _) => "Todos los atajos deben ser válidos y diferentes.",
        ("Close", _) => "Cerrar",
        ("Activated", _) => "Activado",
        ("Deactivated", _) => "Desactivado",
        ("CheckUpdates", _) => "Buscar actualizaciones",
        ("CheckingUpdates", _) => "Buscando actualizaciones…",
        ("UpToDate", _) => "Estás usando la versión más reciente de FocusPocus.",
        ("UpdateAvailable", _) => "Hay una nueva versión de FocusPocus disponible.",
        ("OpenRelease", _) => "Ver y descargar",
        ("UpdateError", _) => "FocusPocus no pudo buscar actualizaciones. Comprueba tu conexión a Internet e inténtalo de nuevo.",
        ("Version", _) => "Versión",
        ("Author", _) => "Autor",
        _ => key
    };

    private void Translate()
    {
        BrandSlogan.Text = T("Subtitle");
        SpotNav.Content = T("Focus"); EffectsNav.Content = T("Effects"); ShortcutsNav.Content = T("Shortcuts"); BehaviorNav.Content = T("Behavior"); AboutNav.Content = T("About");
        SpotEnabledHeader.Text = T("EnableFocus"); SizeLabel.Text = T("FocusSize"); FeatherLabel.Text = T("Feather"); OverlayTitle.Text = T("Overlay"); ColorLabel.Text = T("Color"); ChooseColorButton.Content = T("ChooseColor"); OpacityLabel.Text = T("Opacity");
        EffectsTitle.Text = T("Effects"); ClicksLabel.Text = T("Clicks"); KeysLabel.Text = T("Keys"); OnlyShortcutsLabel.Text = T("OnlyShortcuts"); SoundLabel.Text = T("Sound"); KeySizeLabel.Text = T("KeySize"); PrivacyBar.Message = T("Privacy");
        ShortcutsTitle.Text = T("Shortcuts"); SpotShortcutLabel.Text = T("ToggleFocus"); ClickShortcutLabel.Text = T("ToggleClicks"); KeyShortcutLabel.Text = T("ToggleKeys"); IncreaseSpotLabel.Text = T("IncreaseFocus"); DecreaseSpotLabel.Text = T("DecreaseFocus"); DecreaseOpacityLabel.Text = T("DecreaseOpacity"); IncreaseOpacityLabel.Text = T("IncreaseOpacity");
        BehaviorTitle.Text = T("Behavior"); StartupLabel.Text = T("Startup"); TrayLabel.Text = T("Tray"); StartupInfo.Message = T("StartupInfo"); ApplyButton.Content = T("Apply");
        foreach (var toggle in new[] { SpotEnabled, ClicksEnabled, KeysEnabled, ShortcutsOnly, SoundEnabled, StartupEnabled, TrayEnabled })
        {
            toggle.OnContent = T("Activated");
            toggle.OffContent = T("Deactivated");
        }
        PageTitle.Text = (_currentNav?.Tag as string) switch { "effects" => T("Effects"), "shortcuts" => T("Shortcuts"), "behavior" => T("Behavior"), _ => T("Focus") };
    }

    private void NavigationChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_loading) return;
        if (args.SelectedItemContainer?.Tag as string == "about") { _ = ShowAboutAsync(); sender.SelectedItem = _currentNav; return; }
        if (args.SelectedItemContainer is not NavigationViewItem selected) return;
        _currentNav = selected;
        SpotPage.Visibility = selected.Tag as string == "spot" ? Visibility.Visible : Visibility.Collapsed;
        EffectsPage.Visibility = selected.Tag as string == "effects" ? Visibility.Visible : Visibility.Collapsed;
        ShortcutsPage.Visibility = selected.Tag as string == "shortcuts" ? Visibility.Visible : Visibility.Collapsed;
        BehaviorPage.Visibility = selected.Tag as string == "behavior" ? Visibility.Visible : Visibility.Collapsed;
        Translate();
    }

    private void LanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || LanguageBox.SelectedItem is not ComboBoxItem item) return;
        _settings.Language = item.Tag?.ToString() == "en" ? "en" : "es";
        Translate(); Save();
    }

    private void LiveChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        Pull(); UpdateValues(); Save();
    }

    private void Pull()
    {
        _settings.SpotlightEnabled = SpotEnabled.IsOn; _settings.ClicksEnabled = ClicksEnabled.IsOn; _settings.KeystrokesEnabled = KeysEnabled.IsOn; _settings.ShowShortcutsOnly = ShortcutsOnly.IsOn; _settings.ClickSoundEnabled = SoundEnabled.IsOn; _settings.StartWithWindows = StartupEnabled.IsOn; _settings.MinimizeToTray = TrayEnabled.IsOn;
        _settings.SpotDiameter = SizeSlider.Value; _settings.FeatherSize = FeatherSlider.Value; _settings.KeystrokeFontSize = KeySizeSlider.Value; _settings.OverlayOpacityPercent = OpacitySlider.Value;
        if (TryParseColor(ColorBox.Text, out _)) _settings.OverlayColor = ColorBox.Text.ToUpperInvariant();
        _settings.SpotlightHotkey = SpotShortcut.Text; _settings.ClicksHotkey = ClickShortcut.Text; _settings.KeystrokesHotkey = KeyShortcut.Text; _settings.IncreaseSpotSizeHotkey = IncreaseSpotShortcut.Text; _settings.DecreaseSpotSizeHotkey = DecreaseSpotShortcut.Text; _settings.DecreaseOverlayOpacityHotkey = DecreaseOpacityShortcut.Text; _settings.IncreaseOverlayOpacityHotkey = IncreaseOpacityShortcut.Text;
    }

    private void UpdateValues()
    {
        static double Percent(Slider slider) => Math.Round((slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum) * 100);
        SizeValue.Text = $"{Math.Round(SizeSlider.Value)} px · {Percent(SizeSlider)}%"; FeatherValue.Text = $"{Math.Round(FeatherSlider.Value)} px · {Percent(FeatherSlider)}%"; KeySizeValue.Text = $"{Math.Round(KeySizeSlider.Value)} pt · {Percent(KeySizeSlider)}%"; OpacityValue.Text = $"{Math.Round(OpacitySlider.Value)}%";
    }

    private void ColorTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading) return;
        if (TryParseColor(ColorBox.Text, out var color)) { OverlayColorPicker.Color = color; UpdateColorPreview(); Pull(); Save(); }
    }

    private void ColorPickerChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        if (_loading) return;
        var hexadecimal = $"#{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
        ColorBox.Text = hexadecimal;
        _settings.OverlayColor = hexadecimal;
        ColorPreview.Background = new SolidColorBrush(args.NewColor);
        Save();
    }

    private void UpdateColorPreview()
    {
        if (TryParseColor(ColorBox.Text, out var color)) { ColorPreview.Background = new SolidColorBrush(color); OverlayColorPicker.Color = color; }
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = Colors.Black;
        if (value.Length != 7 || value[0] != '#' || !uint.TryParse(value[1..], System.Globalization.NumberStyles.HexNumber, null, out var rgb)) return false;
        color = ColorHelper.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb); return true;
    }

    private void CaptureShortcut(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        if (e.Key is VirtualKey.Control or VirtualKey.Shift or VirtualKey.Menu or VirtualKey.LeftWindows or VirtualKey.RightWindows) return;
        var parts = new List<string>();
        if (IsDown(VirtualKey.Control)) parts.Add("Ctrl"); if (IsDown(VirtualKey.Shift)) parts.Add("Shift"); if (IsDown(VirtualKey.Menu)) parts.Add("Alt"); if (IsDown(VirtualKey.LeftWindows) || IsDown(VirtualKey.RightWindows)) parts.Add("Win");
        if (parts.Count == 0) return;
        parts.Add(e.Key.ToString());
        ((TextBox)sender).Text = string.Join('+', parts);
        Pull(); Save();
    }

    private static bool IsDown(VirtualKey key) => (InputKeyboardSource.GetKeyStateForCurrentThread(key) & CoreVirtualKeyStates.Down) != 0;

    private async void ApplyClicked(object sender, RoutedEventArgs e)
    {
        Pull();
        if (!TryParseColor(ColorBox.Text, out _)) { await ShowAlertAsync(T("InvalidColor")); return; }
        var shortcuts = new[] { SpotShortcut.Text, ClickShortcut.Text, KeyShortcut.Text, IncreaseSpotShortcut.Text, DecreaseSpotShortcut.Text, DecreaseOpacityShortcut.Text, IncreaseOpacityShortcut.Text };
        if (shortcuts.Any(string.IsNullOrWhiteSpace) || shortcuts.Distinct(StringComparer.OrdinalIgnoreCase).Count() != shortcuts.Length) { await ShowAlertAsync(T("InvalidShortcuts")); return; }
        Save();
    }

    private void Save() => _storage.Save(_settings);

    private async Task CheckUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FocusPocus/2.1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            using var response = await client.GetAsync("https://api.github.com/repos/ezenwa/FocusPocus/releases/latest");
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            var tag = root.GetProperty("tag_name").GetString() ?? "0.0.0";
            var releaseUrl = root.GetProperty("html_url").GetString() ?? "https://github.com/ezenwa/FocusPocus/releases";
            var updateAvailable = Version.TryParse(tag.TrimStart('v', 'V'), out var latest) && latest > new Version(2, 1, 0);
            if (!updateAvailable) { await ShowAlertAsync(T("UpToDate")); return; }
            var dialog = new ContentDialog { XamlRoot = RootGrid.XamlRoot, Title = $"FocusPocus {tag}", Content = T("UpdateAvailable"), PrimaryButtonText = T("OpenRelease"), CloseButtonText = T("Close"), DefaultButton = ContentDialogButton.Primary };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await Launcher.LaunchUriAsync(new Uri(releaseUrl));
        }
        catch { await ShowAlertAsync(T("UpdateError")); }
    }

    private async Task ShowAlertAsync(string message)
    {
        var dialog = new ContentDialog { XamlRoot = RootGrid.XamlRoot, Title = "FocusPocus", Content = message, CloseButtonText = T("Close"), DefaultButton = ContentDialogButton.Close };
        await dialog.ShowAsync();
    }

    private async Task ShowAboutAsync()
    {
        var panel = new StackPanel { Spacing = 10, MinWidth = 340 };
        panel.Children.Add(new Image { Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/FocusPocus.png")), Width = 76, Height = 76, HorizontalAlignment = HorizontalAlignment.Left });
        panel.Children.Add(new TextBlock { Text = "FocusPocus", FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = (Brush)Application.Current.Resources["FocusAccentBrush"] });
        panel.Children.Add(new TextBlock { Text = T("Subtitle"), Foreground = (Brush)Application.Current.Resources["MutedBrush"] });
        panel.Children.Add(new TextBlock { Text = $"{T("Version")}: 2.1.0", Margin = new Thickness(0, 8, 0, 0) });
        panel.Children.Add(new TextBlock { Text = $"{T("Author")}: Joshua Ezenwa", Foreground = (Brush)Application.Current.Resources["MutedBrush"] });
        var dialog = new ContentDialog { XamlRoot = RootGrid.XamlRoot, Title = T("About"), Content = panel, PrimaryButtonText = T("CheckUpdates"), CloseButtonText = T("Close"), DefaultButton = ContentDialogButton.Close };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await CheckUpdatesAsync();
    }
}
