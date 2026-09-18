using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RankOn.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyId = 0x524F;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint VkR = 0x52;

    private HwndSource? _source;
    private IntPtr _handle;
    private Action? _action;

    public void Attach(Window window, Action action)
    {
        _action = action;
        _handle = new WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WndProc);
        RegisterHotKey(_handle, HotkeyId, ModControl | ModShift, VkR);
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            UnregisterHotKey(_handle, HotkeyId);
        }

        _source?.RemoveHook(WndProc);
        _source = null;
        _handle = IntPtr.Zero;
    }

    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == HotkeyId)
        {
            _action?.Invoke();
            handled = true;
        }

        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
