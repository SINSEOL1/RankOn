using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace RankOn.Services;

public sealed class TrayService : IDisposable
{
    private Forms.NotifyIcon? _icon;
    private Window? _window;

    public void Initialize(Window window)
    {
        _window = window;

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("랭크온 열기", null, (_, _) => ShowMainWindow());
        menu.Items.Add("PC 오버레이 표시/숨기기", null, async (_, _) =>
            await App.PcOverlayService.ToggleAsync());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => App.ExitApplication());

        _icon = new Forms.NotifyIcon
        {
            Text = "랭크온",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = menu
        };

        _icon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        if (_window is null)
        {
            return;
        }

        _window.Show();
        if (_window.WindowState == WindowState.Minimized)
        {
            _window.WindowState = WindowState.Normal;
        }

        _window.Activate();
    }

    public void Dispose()
    {
        if (_icon is null)
        {
            return;
        }

        _icon.Visible = false;
        _icon.Dispose();
        _icon = null;
    }
}
