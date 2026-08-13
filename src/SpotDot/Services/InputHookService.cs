using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using SpotDot.Interop;

namespace SpotDot.Services;

public sealed class InputHookService : IDisposable
{
    private readonly NativeMethods.HookProc _mouseProc;
    private readonly NativeMethods.HookProc _keyboardProc;
    private nint _mouseHook;
    private nint _keyboardHook;

    public event Action<bool, int, int>? MouseClicked;
    public event Action<Key, ModifierKeys>? KeyPressed;

    public InputHookService()
    {
        _mouseProc = MouseCallback;
        _keyboardProc = KeyboardCallback;
    }

    public void Start()
    {
        using var process = Process.GetCurrentProcess();
        var module = process.MainModule;
        var handle = NativeMethods.GetModuleHandle(module?.ModuleName);
        _mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _mouseProc, handle, 0);
        _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, handle, 0);
    }

    private nint MouseCallback(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && (wParam == NativeMethods.WM_LBUTTONDOWN || wParam == NativeMethods.WM_RBUTTONDOWN))
        {
            var data = Marshal.PtrToStructure<NativeMethods.MouseHookStruct>(lParam);
            MouseClicked?.Invoke(wParam == NativeMethods.WM_LBUTTONDOWN, data.Point.X, data.Point.Y);
        }
        return NativeMethods.CallNextHookEx(_mouseHook, code, wParam, lParam);
    }

    private nint KeyboardCallback(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && (wParam == NativeMethods.WM_KEYDOWN || wParam == NativeMethods.WM_SYSKEYDOWN))
        {
            var data = Marshal.PtrToStructure<NativeMethods.KeyboardHookStruct>(lParam);
            var key = KeyInterop.KeyFromVirtualKey((int)data.VkCode);
            if (!IsModifier(key)) KeyPressed?.Invoke(key, CurrentModifiers());
        }
        return NativeMethods.CallNextHookEx(_keyboardHook, code, wParam, lParam);
    }

    private static bool IsModifier(Key key) => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin;

    private static ModifierKeys CurrentModifiers()
    {
        ModifierKeys result = ModifierKeys.None;
        if ((NativeMethods.GetAsyncKeyState(0x11) & 0x8000) != 0) result |= ModifierKeys.Control;
        if ((NativeMethods.GetAsyncKeyState(0x10) & 0x8000) != 0) result |= ModifierKeys.Shift;
        if ((NativeMethods.GetAsyncKeyState(0x12) & 0x8000) != 0) result |= ModifierKeys.Alt;
        if ((NativeMethods.GetAsyncKeyState(0x5B) & 0x8000) != 0 || (NativeMethods.GetAsyncKeyState(0x5C) & 0x8000) != 0) result |= ModifierKeys.Windows;
        return result;
    }

    public void Dispose()
    {
        if (_mouseHook != 0) NativeMethods.UnhookWindowsHookEx(_mouseHook);
        if (_keyboardHook != 0) NativeMethods.UnhookWindowsHookEx(_keyboardHook);
    }
}
