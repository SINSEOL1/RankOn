using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class BroadcastPage : UserControl
{
    public BroadcastPage()
    {
        InitializeComponent();

        AddressText.Text = App.BroadcastServer.Address;
        ServerStatusText.Text = App.BroadcastServer.IsRunning
            ? "방송 출력 서버 실행 중"
            : "방송 출력 서버를 시작하지 못했습니다.";
    }

    private void CopyAddress_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(App.BroadcastServer.Address);
    }
}
