using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        App.LocalizationService.ApplyTo(this);
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"RankOn · Version {version?.Major}.{version?.Minor}.{version?.Build}";
    }

    private void CopyDiscord_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText("sinseol");
    }

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        UpdateButton.IsEnabled = false;
        UpdateStatusText.Text = App.LocalizationService.T("업데이트를 확인하는 중입니다.");

        try
        {
            var update = await App.UpdateService.CheckAsync();
            if (update is null)
            {
                UpdateStatusText.Text = App.LocalizationService.T("현재 최신 버전입니다.");
            }
            else
            {
                UpdateStatusText.Text = App.LocalizationService.NewVersion(update.Version);
                App.UpdateService.Open(update);
            }
        }
        catch
        {
            UpdateStatusText.Text = App.LocalizationService.T("업데이트 정보를 확인하지 못했습니다.");
        }
        finally
        {
            UpdateButton.IsEnabled = true;
        }
    }

    private void OpenGitHub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/SINSEOL1/RankOn")
        {
            UseShellExecute = true
        });
    }
}
