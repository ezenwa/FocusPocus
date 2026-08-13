using System.Text.Json.Serialization;

namespace SpotDot.Models;

public sealed class AppSettings
{
    public bool SpotlightEnabled { get; set; }
    public bool ClicksEnabled { get; set; }
    public bool KeystrokesEnabled { get; set; }
    public bool ShowShortcutsOnly { get; set; } = true;
    public bool ClickSoundEnabled { get; set; }
    public bool StartWithWindows { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public string Language { get; set; } = "en";
    public double SpotDiameter { get; set; } = 660;
    public double FeatherSize { get; set; } = 190;
    public double KeystrokeFontSize { get; set; } = 40;
    public byte OverlayOpacity { get; set; } = 166;
    public string OverlayColor { get; set; } = "#000000";
    public string LeftClickColor { get; set; } = "#35C2FF";
    public string RightClickColor { get; set; } = "#FF5C8A";
    public string SpotlightHotkey { get; set; } = "Ctrl+Space";
    public string ClicksHotkey { get; set; } = "Ctrl+Alt+C";
    public string KeystrokesHotkey { get; set; } = "Ctrl+Alt+K";
    public string IncreaseSpotSizeHotkey { get; set; } = "Ctrl+Alt+Up";
    public string DecreaseSpotSizeHotkey { get; set; } = "Ctrl+Alt+Down";
    public string DecreaseOverlayOpacityHotkey { get; set; } = "Ctrl+Alt+Left";
    public string IncreaseOverlayOpacityHotkey { get; set; } = "Ctrl+Alt+Right";

    [JsonIgnore]
    public double OverlayOpacityPercent
    {
        get => Math.Round(OverlayOpacity / 255d * 100);
        set => OverlayOpacity = (byte)Math.Clamp(Math.Round(value / 100d * 255), 0, 255);
    }
}
