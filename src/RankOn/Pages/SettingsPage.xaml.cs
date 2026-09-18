using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class SettingsPage : UserControl
{
    private bool _loaded;

    public SettingsPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        App.LocalizationService.ApplyTo(this);
        var s = App.SettingsService.Current;
        StartWithWindowsCheckBox.IsChecked = s.StartWithWindows;
        StartMinimizedCheckBox.IsChecked = s.StartMinimizedToTray;
        CloseToTrayCheckBox.IsChecked = s.CloseToTray;
        AutoUpdateCheckBox.IsChecked = s.AutoCheckUpdates;

        foreach (var item in LanguageComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, s.Language, StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedItem = item;
                break;
            }
        }

        _loaded = true;
    }

    private async void GeneralSetting_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;

        var s = App.SettingsService.Current;
        s.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        s.StartMinimizedToTray = StartMinimizedCheckBox.IsChecked == true;
        s.CloseToTray = CloseToTrayCheckBox.IsChecked == true;
        s.AutoCheckUpdates = AutoUpdateCheckBox.IsChecked == true;

        App.StartupService.SetEnabled(s.StartWithWindows);
        await App.SettingsService.SaveAsync(s);
    }

    private async void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string theme }) return;

        var s = App.SettingsService.Current;
        s.Theme = theme;
        App.ThemeService.Apply(theme);
        await App.SettingsService.SaveAsync(s);
    }

    private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded || LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string language }) return;

        var s = App.SettingsService.Current;
        s.Language = language;
        App.LocalizationService.Apply(language);
        await App.SettingsService.SaveAsync(s);

        if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.RefreshLocalization();
        }
    }
}
