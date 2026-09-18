using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RankOn.Pages;

public partial class BroadcastPage : UserControl
{
    public BroadcastPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        App.LocalizationService.ApplyTo(this);
        UpdateView();
    }

    private async void ToggleServer_Click(object sender, RoutedEventArgs e)
    {
        var settings = App.SettingsService.Current;

        try
        {
            if (App.BroadcastServer.IsRunning)
            {
                await App.BroadcastServer.StopAsync();
                settings.BroadcastEnabled = false;
            }
            else
            {
                await App.BroadcastServer.StartAsync();
                settings.BroadcastEnabled = true;
            }

            await App.SettingsService.SaveAsync(settings);
        }
        catch
        {
        }

        UpdateView();
    }

    private void CopyAddress_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(App.BroadcastServer.Address);
    }

    private void OpenPreview_Click(object sender, RoutedEventArgs e)
    {
        if (!App.BroadcastServer.IsRunning) return;

        Process.Start(new ProcessStartInfo(App.BroadcastServer.Address)
        {
            UseShellExecute = true
        });
    }

    private void UpdateView()
    {
        var running = App.BroadcastServer.IsRunning;
        AddressText.Text = App.BroadcastServer.Address;
        ServerStatusText.Text = running
            ? App.LocalizationService.T("방송 출력 실행 중")
            : string.IsNullOrWhiteSpace(App.BroadcastServer.LastError)
                ? App.LocalizationService.T("방송 출력 꺼짐")
                : App.LocalizationService.T("방송 출력 시작 실패");
        ServerStatusText.Foreground = running
            ? (Brush)FindResource("PositiveBrush")
            : (Brush)FindResource("MutedTextBrush");
        ToggleServerButton.Content = running
            ? App.LocalizationService.T("출력 끄기")
            : App.LocalizationService.T("출력 켜기");
    }
}
