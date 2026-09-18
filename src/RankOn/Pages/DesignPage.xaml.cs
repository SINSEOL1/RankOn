using System.Windows;
using System.Windows.Controls;

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
        var mode = App.SettingsService.Current.TargetRpDisplayMode;

        foreach (var item in TargetRpDisplayComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, mode, StringComparison.OrdinalIgnoreCase))
            {
                TargetRpDisplayComboBox.SelectedItem = item;
                break;
            }
        }

        if (TargetRpDisplayComboBox.SelectedItem is null)
        {
            TargetRpDisplayComboBox.SelectedIndex = 0;
        }

        _loaded = true;
    }

    private async void TargetRpDisplayComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_loaded ||
            TargetRpDisplayComboBox.SelectedItem is not ComboBoxItem item ||
            item.Tag is not string mode)
        {
            return;
        }

        var settings = App.SettingsService.Current;
        settings.TargetRpDisplayMode = mode;
        await App.SettingsService.SaveAsync(settings);
        App.RankPollingService.RebuildOverlayState();
    }
}
