using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RankOn.Pages;

public partial class DesignPage : UserControl
{
    private bool _loaded;

    public DesignPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        LoadControls();
        _loaded = true;
        UpdatePreview();
    }

    private async void Preset_Checked(object sender, RoutedEventArgs e)
    {
        if (!_loaded || sender is not RadioButton { Tag: string preset }) return;

        var s = App.SettingsService.Current;
        s.OverlayPreset = preset;

        switch (preset)
        {
            case "Compact":
                s.OverlayBackgroundEnabled = true; s.OverlayBackgroundOpacity = 0.82; s.OverlayCornerRadius = 10; s.OverlayFontScale = 0.88; s.OverlayShowSeason = false; s.OverlayShowTarget = false; break;
            case "Minimal":
                s.OverlayBackgroundEnabled = false; s.OverlayFontScale = 0.95; s.OverlayShowNickname = false; s.OverlayShowSeason = false; s.OverlayShowTarget = false; break;
            case "Vertical":
                s.OverlayBackgroundEnabled = true; s.OverlayBackgroundOpacity = 0.88; s.OverlayCornerRadius = 18; s.OverlayFontScale = 1.0; s.OverlayShowNickname = true; s.OverlayShowSeason = true; s.OverlayShowTarget = true; break;
            case "StreamBar":
                s.OverlayBackgroundEnabled = true; s.OverlayBackgroundOpacity = 0.76; s.OverlayCornerRadius = 8; s.OverlayFontScale = 0.9; s.OverlayShowNickname = true; s.OverlayShowSeason = false; s.OverlayShowTarget = true; break;
            default:
                s.OverlayBackgroundEnabled = true; s.OverlayBackgroundOpacity = 0.88; s.OverlayCornerRadius = 14; s.OverlayFontScale = 1.0; s.OverlayShowNickname = true; s.OverlayShowSeason = true; s.OverlayShowTarget = true; break;
        }

        s.OverlayShowTier = true;
        s.OverlayShowRp = true;
        s.OverlayShowRank = true;
        s.OverlayShowSession = true;

        LoadControls();
        await SaveApplyAsync();
    }

    private async void DesignControl_Changed(object sender, EventArgs e)
    {
        if (!_loaded) return;

        var s = App.SettingsService.Current;
        s.OverlayBackgroundEnabled = BackgroundCheckBox.IsChecked == true;
        s.OverlayBackgroundOpacity = BackgroundOpacitySlider.Value / 100.0;
        s.OverlayCornerRadius = CornerRadiusSlider.Value;
        s.OverlayFontScale = FontScaleSlider.Value / 100.0;
        s.PcOverlayScale = s.OverlayFontScale;
        s.OverlayShowNickname = ShowNicknameCheckBox.IsChecked == true;
        s.OverlayShowTier = ShowTierCheckBox.IsChecked == true;
        s.OverlayShowRp = ShowRpCheckBox.IsChecked == true;
        s.OverlayShowRank = ShowRankCheckBox.IsChecked == true;
        s.OverlayShowSession = ShowSessionCheckBox.IsChecked == true;
        s.OverlayShowSeason = ShowSeasonCheckBox.IsChecked == true;
        s.OverlayShowTarget = ShowTargetCheckBox.IsChecked == true;

        if (TargetRpDisplayComboBox.SelectedItem is ComboBoxItem { Tag: string mode })
        {
            s.TargetRpDisplayMode = mode;
        }

        await SaveApplyAsync();
    }

    private void LoadControls()
    {
        var s = App.SettingsService.Current;
        var previousLoaded = _loaded;
        _loaded = false;

        StandardPreset.IsChecked = s.OverlayPreset == "Standard";
        CompactPreset.IsChecked = s.OverlayPreset == "Compact";
        MinimalPreset.IsChecked = s.OverlayPreset == "Minimal";
        VerticalPreset.IsChecked = s.OverlayPreset == "Vertical";
        StreamBarPreset.IsChecked = s.OverlayPreset == "StreamBar";
        BackgroundCheckBox.IsChecked = s.OverlayBackgroundEnabled;
        BackgroundOpacitySlider.Value = s.OverlayBackgroundOpacity * 100;
        CornerRadiusSlider.Value = s.OverlayCornerRadius;
        FontScaleSlider.Value = s.OverlayFontScale * 100;
        ShowNicknameCheckBox.IsChecked = s.OverlayShowNickname;
        ShowTierCheckBox.IsChecked = s.OverlayShowTier;
        ShowRpCheckBox.IsChecked = s.OverlayShowRp;
        ShowRankCheckBox.IsChecked = s.OverlayShowRank;
        ShowSessionCheckBox.IsChecked = s.OverlayShowSession;
        ShowSeasonCheckBox.IsChecked = s.OverlayShowSeason;
        ShowTargetCheckBox.IsChecked = s.OverlayShowTarget;

        foreach (var item in TargetRpDisplayComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, s.TargetRpDisplayMode, StringComparison.OrdinalIgnoreCase))
            {
                TargetRpDisplayComboBox.SelectedItem = item;
                break;
            }
        }

        _loaded = previousLoaded;
    }

    private async Task SaveApplyAsync()
    {
        await App.SettingsService.SaveAsync(App.SettingsService.Current);
        App.RankPollingService.RebuildOverlayState();
        App.PcOverlayService.ApplySettings();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var s = App.SettingsService.Current;
        PreviewBorder.Background = s.OverlayBackgroundEnabled
            ? new SolidColorBrush(Color.FromArgb((byte)Math.Round(s.OverlayBackgroundOpacity * 255), 16, 18, 24))
            : Brushes.Transparent;
        PreviewBorder.CornerRadius = new CornerRadius(s.OverlayCornerRadius);
        PreviewBorder.LayoutTransform = new ScaleTransform(s.OverlayFontScale, s.OverlayFontScale);

        var vertical = s.OverlayPreset == "Vertical";
        PreviewPanel.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;
        PreviewBadge.Margin = vertical ? new Thickness(0, 0, 0, 10) : new Thickness(0);
        PreviewInfo.Margin = vertical ? new Thickness(0) : new Thickness(16, 0, 24, 0);
        PreviewSession.Margin = vertical ? new Thickness(0, 10, 0, 0) : new Thickness(0);

        PreviewNickname.Visibility = BoolVisibility(s.OverlayShowNickname);
        PreviewTier.Visibility = BoolVisibility(s.OverlayShowTier);
        PreviewRp.Visibility = BoolVisibility(s.OverlayShowRp);
        PreviewRank.Visibility = BoolVisibility(s.OverlayShowRank);
        PreviewSession.Visibility = BoolVisibility(s.OverlayShowSession);
        PreviewSeason.Visibility = BoolVisibility(s.OverlayShowSeason);

        PreviewTarget.Text = s.TargetRpDisplayMode switch
        {
            "Demigod" => "데미갓까지 240 RP",
            "Eternity" => "이터니티까지 420 RP",
            _ => ""
        };
        PreviewTarget.Visibility = BoolVisibility(s.OverlayShowTarget && !string.IsNullOrWhiteSpace(PreviewTarget.Text));
    }

    private static Visibility BoolVisibility(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;
}
