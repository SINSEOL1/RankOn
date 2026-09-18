using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using RankOn.Models;
using RankOn.Services;

namespace RankOn.Windows;

public partial class OverlayWindow : Window
{
    private bool _positionMode;
    private bool _locked = true;
    private bool _clickThrough = true;

    public OverlayWindow()
    {
        InitializeComponent();
    }

    public event EventHandler? PositionCommitted;

    public void SetState(OverlayState state)
    {
        RootBorder.Visibility = state.HasData ? Visibility.Visible : Visibility.Collapsed;

        if (!state.HasData)
        {
            return;
        }

        NicknameText.Text = state.Nickname;
        TierText.Text = state.Tier;
        RpText.Text = $"{state.Rp:N0} RP";
        RankText.Text = state.Rank > 0 ? $"#{state.Rank:N0}" : "";
        SessionText.Text = $"{(state.SessionDelta > 0 ? "+" : "")}{state.SessionDelta:N0} RP";
        SeasonText.Text = state.SeasonRemaining;
        TargetText.Text = state.TargetRpText;
        RankIconImage.Source = TierIconService.GetImage(state.TierKey);
        BadgeBorder.Visibility = RankIconImage.Source is null ? Visibility.Collapsed : Visibility.Visible;

        NicknameText.Visibility = state.ShowNickname ? Visibility.Visible : Visibility.Collapsed;
        TierText.Visibility = state.ShowTier ? Visibility.Visible : Visibility.Collapsed;
        RpText.Visibility = state.ShowRp ? Visibility.Visible : Visibility.Collapsed;
        RankText.Visibility = state.ShowRank ? Visibility.Visible : Visibility.Collapsed;
        SessionText.Visibility = state.ShowSession ? Visibility.Visible : Visibility.Collapsed;
        SeasonText.Visibility = state.ShowSeason && !string.IsNullOrWhiteSpace(state.SeasonRemaining)
            ? Visibility.Visible
            : Visibility.Collapsed;
        TargetText.Visibility = state.ShowTarget && !string.IsNullOrWhiteSpace(state.TargetRpText)
            ? Visibility.Visible
            : Visibility.Collapsed;

        var vertical = string.Equals(state.Preset, "Vertical", StringComparison.OrdinalIgnoreCase);
        ContentPanel.Orientation = vertical ? System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal;
        ContentPanel.HorizontalAlignment = vertical ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        BadgeBorder.Margin = vertical ? new Thickness(0, 0, 0, 10) : new Thickness(0, 0, 14, 0);
        InfoPanel.HorizontalAlignment = vertical ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        NicknameText.TextAlignment = vertical ? TextAlignment.Center : TextAlignment.Left;
        RankLinePanel.HorizontalAlignment = vertical ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        SeasonText.TextAlignment = vertical ? TextAlignment.Center : TextAlignment.Left;
        TargetText.TextAlignment = vertical ? TextAlignment.Center : TextAlignment.Left;
        SessionText.Margin = vertical ? new Thickness(0, 10, 0, 0) : new Thickness(20, 0, 0, 0);

        var scale = Math.Clamp(state.FontScale, 0.7, 1.6);
        RootBorder.LayoutTransform = new ScaleTransform(scale, scale);
        RootBorder.CornerRadius = new CornerRadius(Math.Clamp(state.CornerRadius, 0, 40));

        var backgroundAlpha = state.BackgroundEnabled
            ? (byte)Math.Round(Math.Clamp(state.BackgroundOpacity, 0, 1) * 255)
            : (byte)0;

        RootBorder.Background = new SolidColorBrush(Color.FromArgb(backgroundAlpha, 16, 18, 24));
        RootBorder.BorderBrush = state.BackgroundEnabled
            ? new SolidColorBrush(Color.FromArgb((byte)Math.Min(90, (int)backgroundAlpha), 255, 255, 255))
            : Brushes.Transparent;

        BadgeBorder.Background = Brushes.Transparent;
        SessionText.Foreground = state.SessionDelta >= 0
            ? new SolidColorBrush(Color.FromRgb(112, 214, 166))
            : new SolidColorBrush(Color.FromRgb(255, 142, 142));
    }

    public void ApplyWindowSettings(AppSettings settings)
    {
        Topmost = settings.PcOverlayTopmost;
        Opacity = Math.Clamp(settings.PcOverlayOpacity, 0.2, 1.0);
        var localScale = Math.Clamp(settings.PcOverlayScale, 0.5, 1.6);
        ScaleContainer.LayoutTransform = new ScaleTransform(localScale, localScale);
        _locked = settings.PcOverlayLocked;
        _clickThrough = settings.PcOverlayClickThrough;

        if (settings.PcOverlayX is double x)
        {
            Left = x;
        }

        if (settings.PcOverlayY is double y)
        {
            Top = y;
        }

        ApplyInteraction();
    }

    public void SetPositionMode(bool enabled)
    {
        _positionMode = enabled;
        PositionHint.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        ApplyInteraction();
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_positionMode && _locked)
        {
            return;
        }

        try
        {
            DragMove();
            PositionCommitted?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
        }
    }

    private void ApplyInteraction()
    {
        if (!IsLoaded)
        {
            SourceInitialized -= OverlayWindow_SourceInitialized;
            SourceInitialized += OverlayWindow_SourceInitialized;
            return;
        }

        SetClickThrough(!_positionMode && _locked && _clickThrough);
    }

    private void OverlayWindow_SourceInitialized(object? sender, EventArgs e)
    {
        SourceInitialized -= OverlayWindow_SourceInitialized;
        ApplyToolWindowStyle();
        SetClickThrough(!_positionMode && _locked && _clickThrough);
    }

    private void ApplyToolWindowStyle()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(handle, -20).ToInt64();
        style |= 0x80;
        style &= ~0x40000L;
        SetWindowLongPtr(handle, -20, new IntPtr(style));
    }

    private void SetClickThrough(bool enabled)
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(handle, -20).ToInt64();

        if (enabled)
        {
            style |= 0x20;
        }
        else
        {
            style &= ~0x20L;
        }

        SetWindowLongPtr(handle, -20, new IntPtr(style));
    }

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int index)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, index)
            : new IntPtr(GetWindowLong32(hWnd, index));
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int index, IntPtr value)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, index, value)
            : new IntPtr(SetWindowLong32(hWnd, index, value.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int index, int value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int index, IntPtr value);
}
