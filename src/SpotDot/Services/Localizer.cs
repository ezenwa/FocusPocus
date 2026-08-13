namespace SpotDot.Services;

public sealed class Localizer
{
    private readonly Dictionary<string, (string Es, string En)> _text = new()
    {
        ["AppSubtitle"] = ("Lleva la atención justo donde importa", "Bring attention exactly where it matters"),
        ["Spotlight"] = ("Foco", "Spotlight"),
        ["EnableSpotlight"] = ("Activar foco", "Enable spotlight"),
        ["SpotSize"] = ("Tamaño del foco", "Spot size"),
        ["Feather"] = ("Difuminado del contorno", "Edge feather"),
        ["OverlayColor"] = ("Color del overlay", "Overlay color"),
        ["ChooseColor"] = ("Elegir color…", "Choose color…"),
        ["Opacity"] = ("Opacidad", "Opacity"),
        ["InputEffects"] = ("Efectos de entrada", "Input effects"),
        ["ShowClicks"] = ("Mostrar clics", "Show mouse clicks"),
        ["ShowKeys"] = ("Mostrar teclas pulsadas", "Show keystrokes"),
        ["ShowShortcutsOnly"] = ("Solo mostrar atajos", "Only show shortcuts"),
        ["KeySize"] = ("Tamaño de las teclas", "Keystroke size"),
        ["ClickSound"] = ("Reproducir sonido al hacer clic", "Play a sound on click"),
        ["PrivacyHint"] = ("Las teclas se ocultan en campos de contraseña.", "Keystrokes are hidden in password fields."),
        ["Behavior"] = ("Comportamiento", "Behavior"),
        ["StartWindows"] = ("Iniciar con Windows", "Start with Windows"),
        ["Tray"] = ("Cerrar la ventana a la bandeja", "Close window to system tray"),
        ["Language"] = ("Idioma", "Language"),
        ["Shortcuts"] = ("Atajos globales", "Global shortcuts"),
        ["CaptureHint"] = ("Haz clic en un campo y pulsa la combinación deseada.", "Click a field and press the desired combination."),
        ["Apply"] = ("Aplicar", "Apply"),
        ["About"] = ("Acerca de", "About"),
        ["Exit"] = ("Salir", "Exit"),
        ["Show"] = ("Mostrar FocusPocus", "Show FocusPocus"),
        ["ToggleSpotlight"] = ("Alternar foco", "Toggle spotlight"),
        ["ToggleClicks"] = ("Alternar clics", "Toggle clicks"),
        ["ToggleKeys"] = ("Alternar teclas", "Toggle keystrokes"),
        ["IncreaseSpotSize"] = ("Aumentar foco", "Increase spotlight size"),
        ["DecreaseSpotSize"] = ("Disminuir foco", "Decrease spotlight size"),
        ["DecreaseOverlayOpacity"] = ("Reducir opacidad", "Decrease overlay opacity"),
        ["IncreaseOverlayOpacity"] = ("Aumentar opacidad", "Increase overlay opacity"),
        ["Saved"] = ("Configuración guardada", "Settings saved"),
        ["InvalidColor"] = ("Introduce colores hexadecimales válidos.", "Enter valid hexadecimal colors."),
        ["ShortcutConflict"] = ("Los atajos deben ser válidos y diferentes.", "Shortcuts must be valid and unique."),
        ["Author"] = ("Autor", "Author"),
        ["Version"] = ("Versión", "Version"),
        ["Close"] = ("Cerrar", "Close")
        ,["Accept"] = ("Aceptar", "OK")
    };

    public string Language { get; set; }
    public Localizer(string language) => Language = language;
    public string this[string key] => _text.TryGetValue(key, out var value) ? (Language == "en" ? value.En : value.Es) : key;
}
