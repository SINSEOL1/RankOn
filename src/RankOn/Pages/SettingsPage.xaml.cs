using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string theme)
        {
            return;
        }

        var settings = App.SettingsService.Current;
        settings.Theme = theme;
        App.ThemeService.Apply(theme);
        await App.SettingsService.SaveAsync(settings);
    }
}
