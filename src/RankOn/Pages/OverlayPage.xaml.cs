using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class OverlayPage : UserControl
{
    private bool _loaded;

    public OverlayPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        App.PcOverlayService.Changed += PcOverlayService_Changed;
        LoadSettings();
        _loaded = true;
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        App.PcOverlayService.Changed -= PcOverlayService_Changed;
        _loaded = false;
    }

    private void PcOverlayService_Changed(object? sender, EventArgs e) => Dispatcher.Invoke(UpdatePositionView);

    private void LoadSettings()
    {
        var settings = App.SettingsService.Current;
        EnabledCheckBox.IsChecked = settings.PcOverlayEnabled;
        TopmostCheckBox.IsChecked = settings.PcOverlayTopmost;
        ClickThroughCheckBox.IsChecked = settings.PcOverlayClickThrough;
        LockCheckBox.IsChecked = settings.PcOverlayLocked;
        ScaleSlider.Value = settings.PcOverlayScale * 100;
        OpacitySlider.Value = settings.PcOverlayOpacity * 100;
        UpdatePositionView();
    }

    private async void EnabledCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        await App.PcOverlayService.SetEnabledAsync(EnabledCheckBox.IsChecked == true);
        UpdatePositionView();
    }

    private async void PositionModeButton_Click(object sender, RoutedEventArgs e)
    {
        await App.PcOverlayService.SetPositionModeAsync(!App.PcOverlayService.IsPositionMode);
        UpdatePositionView();
    }

    private async void ResetPosition_Click(object sender, RoutedEventArgs e)
    {
        await App.PcOverlayService.ResetPositionAsync();
        UpdatePositionView();
    }

    private async void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;

        var settings = App.SettingsService.Current;
        settings.PcOverlayScale = ScaleSlider.Value / 100.0;
        await App.SettingsService.SaveAsync(settings);
        App.PcOverlayService.ApplySettings();
        UpdatePositionView();
    }

    private async void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded) return;

        var settings = App.SettingsService.Current;
        settings.PcOverlayOpacity = OpacitySlider.Value / 100.0;
        await App.SettingsService.SaveAsync(settings);
        App.PcOverlayService.ApplySettings();
        UpdatePositionView();
    }

    private async void WindowOption_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;

        var settings = App.SettingsService.Current;
        settings.PcOverlayTopmost = TopmostCheckBox.IsChecked == true;
        settings.PcOverlayClickThrough = ClickThroughCheckBox.IsChecked == true;
        settings.PcOverlayLocked = LockCheckBox.IsChecked == true;
        await App.SettingsService.SaveAsync(settings);
        App.PcOverlayService.ApplySettings();
    }

    private void UpdatePositionView()
    {
        var settings = App.SettingsService.Current;
        ScaleValueText.Text = $"{settings.PcOverlayScale * 100:0}%";
        OpacityValueText.Text = $"{settings.PcOverlayOpacity * 100:0}%";
        PositionText.Text = settings.PcOverlayX is double x && settings.PcOverlayY is double y
            ? $"X {x:0} · Y {y:0}"
            : "기본 위치";
        PositionModeButton.Content = App.PcOverlayService.IsPositionMode ? "위치 조정 완료" : "위치 조정";
    }
}
