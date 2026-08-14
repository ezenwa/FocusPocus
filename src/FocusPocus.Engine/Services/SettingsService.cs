using System.IO;
using System.Text.Json;
using FocusPocus.Engine.Models;

namespace FocusPocus.Engine.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;
    public string SettingsPath => _path;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "FocusPocus");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            var settings = File.Exists(_path)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), JsonOptions) ?? new AppSettings()
                : new AppSettings();
            var migrated = false;
            if (settings.SpotlightHotkey.Equals("Ctrl+Shift+F8", StringComparison.OrdinalIgnoreCase))
            { settings.SpotlightHotkey = "Ctrl+Alt+Space"; migrated = true; }
            if (settings.ClicksHotkey.Equals("Ctrl+Shift+F9", StringComparison.OrdinalIgnoreCase))
            { settings.ClicksHotkey = "Ctrl+Alt+C"; migrated = true; }
            if (settings.KeystrokesHotkey.Equals("Ctrl+Shift+F10", StringComparison.OrdinalIgnoreCase))
            { settings.KeystrokesHotkey = "Ctrl+Alt+K"; migrated = true; }
            if (migrated) Save(settings);
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var temporary = _path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporary, _path, true);
    }
}
