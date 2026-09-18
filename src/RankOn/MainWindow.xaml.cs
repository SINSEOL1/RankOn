using System.Windows;
using System.Windows.Controls;
using RankOn.Pages;

namespace RankOn;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Navigate("Home");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        App.RankPollingService.Changed += RankPollingService_Changed;
        UpdateSidebar();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        App.RankPollingService.Changed -= RankPollingService_Changed;
    }

    private void RankPollingService_Changed(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(UpdateSidebar);
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string page)
        {
            Navigate(page);
        }
    }

    private void Navigate(string page)
    {
        PageContent.Content = page switch
        {
            "Broadcast" => new BroadcastPage(),
            "Overlay" => new OverlayPage(),
            "Design" => new DesignPage(),
            "Profiles" => new ProfilesPage(),
            "Settings" => new SettingsPage(),
            "About" => new AboutPage(),
            _ => new HomePage()
        };

        PageTitle.Text = page switch
        {
            "Broadcast" => "방송 출력",
            "Overlay" => "오버레이",
            "Design" => "디자인",
            "Profiles" => "프로필",
            "Settings" => "설정",
            "About" => "정보",
            _ => "홈"
        };
    }

    private void UpdateSidebar()
    {
        var profile = App.RankPollingService.CurrentProfile;
        var snapshot = App.RankPollingService.Current;

        if (profile is null)
        {
            SidebarNicknameText.Text = "프로필 미등록";
            SidebarRankText.Text = "닉네임을 등록해 시작하세요";
            return;
        }

        SidebarNicknameText.Text = profile.Nickname;

        SidebarRankText.Text = snapshot is null
            ? "랭크 정보를 불러오는 중"
            : $"{snapshot.TierDisplayName} · {snapshot.Rp:N0} RP";
    }
}
