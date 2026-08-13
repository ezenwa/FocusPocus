using System.Media;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SpotDot.Models;
using SpotDot.Views;
using Forms = System.Windows.Forms;

namespace SpotDot.Services;

public sealed class AppController : IDisposable
{
    private readonly SettingsService _storage=new(); private readonly InputHookService _hooks=new(); private readonly System.Windows.Threading.DispatcherTimer _syncTimer=new(){Interval=TimeSpan.FromMilliseconds(400)}; private OverlayWindow _overlay=null!; private Forms.NotifyIcon _tray=null!; private DateTime _settingsWriteTimeUtc;
    public AppSettings Settings{get;private set;}=null!; public Localizer Localizer{get;private set;}=null!; public bool IsExiting{get;private set;}
    public void Start(bool startInTray=false){Settings=_storage.Load();Localizer=new(Settings.Language);SetStartup(Settings.StartWithWindows);SpotDot.Interop.NativeMethods.GetCursorPos(out var p);_overlay=new OverlayWindow(Settings,Forms.Screen.FromPoint(new System.Drawing.Point(p.X,p.Y)));_hooks.MouseClicked+=OnClick;_hooks.KeyPressed+=OnKey;_hooks.Start();CreateTray();ApplyLive();_settingsWriteTimeUtc=File.Exists(_storage.SettingsPath)?File.GetLastWriteTimeUtc(_storage.SettingsPath):DateTime.MinValue;_syncTimer.Tick+=(_,_)=>SyncExternalState();_syncTimer.Start();}
    private void SyncExternalState(){var writeTime=File.Exists(_storage.SettingsPath)?File.GetLastWriteTimeUtc(_storage.SettingsPath):DateTime.MinValue;if(writeTime>_settingsWriteTimeUtc){Settings=_storage.Load();Localizer.Language=Settings.Language;_settingsWriteTimeUtc=writeTime;SetStartup(Settings.StartWithWindows);ApplyLive();RefreshTray();}var uiVisible=Process.GetProcessesByName("FocusPocus").Any();_overlay.Topmost=!uiVisible;}
    public void ApplyLive(){try{_overlay.ApplySettings(Settings);_overlay.SetSpotlight(Settings.SpotlightEnabled);}catch{} }
    private void OnClick(bool left,int x,int y)=>System.Windows.Application.Current.Dispatcher.Invoke(()=>{if(!Settings.ClicksEnabled)return;_overlay.MoveToScreen(Forms.Screen.FromPoint(new System.Drawing.Point(x,y)));_overlay.ShowClick(left,x,y,left?Settings.LeftClickColor:Settings.RightClickColor);if(Settings.ClickSoundEnabled)SystemSounds.Asterisk.Play();});
    private void OnKey(Key key,ModifierKeys mods)=>System.Windows.Application.Current.Dispatcher.Invoke(()=>{var text=HotkeyService.Format(mods,key);if(Matches(text,Settings.SpotlightHotkey)){Settings.SpotlightEnabled=!Settings.SpotlightEnabled;ApplyLive();return;}if(Matches(text,Settings.ClicksHotkey)){Settings.ClicksEnabled=!Settings.ClicksEnabled;return;}if(Matches(text,Settings.KeystrokesHotkey)){Settings.KeystrokesEnabled=!Settings.KeystrokesEnabled;return;}if(Matches(text,Settings.IncreaseSpotSizeHotkey)){ChangeSpotSize(10);return;}if(Matches(text,Settings.DecreaseSpotSizeHotkey)){ChangeSpotSize(-10);return;}if(Matches(text,Settings.DecreaseOverlayOpacityHotkey)){ChangeOverlayOpacity(-10);return;}if(Matches(text,Settings.IncreaseOverlayOpacityHotkey)){ChangeOverlayOpacity(10);return;}if(Settings.KeystrokesEnabled&&(!Settings.ShowShortcutsOnly||mods!=ModifierKeys.None)&&!IsPasswordField())_overlay.ShowKey(text);});
    private void ChangeSpotSize(double percentagePoints){const double minimum=100,maximum=800;var change=(maximum-minimum)*percentagePoints/100;Settings.SpotDiameter=Math.Clamp(Settings.SpotDiameter+change,minimum,maximum);ApplyLive();SaveSettings();}
    private void ChangeOverlayOpacity(double percentagePoints){Settings.OverlayOpacityPercent=Math.Clamp(Settings.OverlayOpacityPercent+percentagePoints,0,100);ApplyLive();SaveSettings();}
    private static bool Matches(string a,string b)=>string.Equals(a,b,StringComparison.OrdinalIgnoreCase);
    private static bool IsPasswordField(){var hwnd=SpotDot.Interop.NativeMethods.GetForegroundWindow();var thread=SpotDot.Interop.NativeMethods.GetWindowThreadProcessId(hwnd,out _);var info=new SpotDot.Interop.NativeMethods.GuiThreadInfo{Size=System.Runtime.InteropServices.Marshal.SizeOf<SpotDot.Interop.NativeMethods.GuiThreadInfo>()};if(!SpotDot.Interop.NativeMethods.GetGUIThreadInfo(thread,ref info)||info.Focus==0)return false;var style=SpotDot.Interop.NativeMethods.GetWindowLongPtr(info.Focus,SpotDot.Interop.NativeMethods.GWL_STYLE).ToInt64();return(style&SpotDot.Interop.NativeMethods.ES_PASSWORD)!=0;}
    public bool ValidateAndSave(out string error){error="";try{_ = System.Windows.Media.ColorConverter.ConvertFromString(Settings.OverlayColor);}catch{error=Localizer["InvalidColor"];return false;}var h=new[]{Settings.SpotlightHotkey,Settings.ClicksHotkey,Settings.KeystrokesHotkey,Settings.IncreaseSpotSizeHotkey,Settings.DecreaseSpotSizeHotkey,Settings.DecreaseOverlayOpacityHotkey,Settings.IncreaseOverlayOpacityHotkey};if(h.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=h.Length||h.Any(x=>!HotkeyService.TryParse(x,out _,out _))){error=Localizer["ShortcutConflict"];return false;}SetStartup(Settings.StartWithWindows);SaveSettings();ApplyLive();return true;}
    private void SaveSettings(){_storage.Save(Settings);_settingsWriteTimeUtc=File.GetLastWriteTimeUtc(_storage.SettingsPath);}
    private static void SetStartup(bool enabled){using var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true);key?.DeleteValue("SpotDot",false);if(enabled)key?.SetValue("FocusPocus",$"\"{Environment.ProcessPath}\" --tray");else key?.DeleteValue("FocusPocus",false);}
    private void CreateTray(){var iconPath=Path.Combine(AppContext.BaseDirectory,"Assets","FocusPocus-Tray.ico");_tray=new Forms.NotifyIcon{Text="FocusPocus",Icon=File.Exists(iconPath)?new System.Drawing.Icon(iconPath):System.Drawing.SystemIcons.Information,Visible=true};_tray.DoubleClick+=(_,_)=>ShowMain();RefreshTray();}
    public void RefreshTray(){var m=new Forms.ContextMenuStrip();m.Items.Add(Localizer["Show"],null,(_,_)=>ShowMain());m.Items.Add(Localizer["ToggleSpotlight"],null,(_,_)=>{Settings.SpotlightEnabled=!Settings.SpotlightEnabled;ApplyLive();});m.Items.Add(new Forms.ToolStripSeparator());m.Items.Add(Localizer["Exit"],null,(_,_)=>Exit());_tray.ContextMenuStrip=m;}
    private void ShowMain(){var uiPath=Path.Combine(AppContext.BaseDirectory,"FocusPocus.exe");if(File.Exists(uiPath))Process.Start(new ProcessStartInfo(uiPath){UseShellExecute=true});}
    private void Exit(){IsExiting=true;SaveSettings();System.Windows.Application.Current.Shutdown();}
    public void Dispose(){_syncTimer.Stop();_hooks.Dispose();if(_tray!=null){_tray.Visible=false;_tray.Dispose();}}
}
