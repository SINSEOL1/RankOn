using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using RankOn.Pages;
using RankOn.Services;

namespace RankOn;

public partial class MainWindow : Window
{
    private readonly GlobalHotkeyService _hotkeyService = new();

    public MainWindow()
    {
        InitializeComponent();
        Navigate("Home");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        App.RankPollingService.Changed += RankPollingService_Changed;
        App.PcOverlayService.Changed += PcOverlayService_Changed;
        _hotkeyService.Attach(this, () => _ = App.PcOverlayService.ToggleAsync());
        App.LocalizationService.ApplyTo(this);
        UpdateSidebar();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (App.IsExiting)
        {
            return;
        }

        if (App.SettingsService.Current.CloseToTray)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        e.Cancel = true;
        App.ExitApplication();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        App.RankPollingService.Changed -= RankPollingService_Changed;
        App.PcOverlayService.Changed -= PcOverlayService_Changed;
        _hotkeyService.Dispose();
    }

    private void RankPollingService_Changed(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(UpdateSidebar);
    }

    private void PcOverlayService_Changed(object? sender, EventArgs e)
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

        var title = page switch
        {
            "Broadcast" => "방송 출력",
            "Overlay" => "오버레이",
            "Design" => "디자인",
            "Profiles" => "프로필",
            "Settings" => "설정",
            "About" => "정보",
            _ => "홈"
        };

        PageTitle.Text = App.LocalizationService.T(title);

        if (PageContent.Content is DependencyObject content)
        {
            App.LocalizationService.ApplyTo(content);
        }
    }

    public void RefreshLocalization()
    {
        App.LocalizationService.ApplyTo(this);
        Navigate(PageTitle.Text switch
        {
            "Broadcast" or "配信出力" or "直播输出" => "Broadcast",
            "Overlay" or "オーバーレイ" or "悬浮层" => "Overlay",
            "Design" or "デザイン" or "设计" => "Design",
            "Profile" or "プロフィール" or "资料" => "Profiles",
            "Settings" or "設定" or "设置" => "Settings",
            "About" or "情報" or "关于" => "About",
            _ => "Home"
        });
        UpdateSidebar();
    }

    private void UpdateSidebar()
    {
        var profile = App.RankPollingService.CurrentProfile;
        var snapshot = App.RankPollingService.Current;

        if (profile is null)
        {
            SidebarNicknameText.Text = App.LocalizationService.T("프로필 미등록");
            SidebarRankText.Text = App.LocalizationService.T("닉네임을 등록해 시작하세요");
            return;
        }

        SidebarNicknameText.Text = profile.Nickname;
        SidebarRankText.Text = snapshot is null
            ? App.LocalizationService.T("랭크 정보를 불러오는 중입니다.")
            : $"{App.LocalizationService.Tier(snapshot.TierKey, snapshot.Division)} · {snapshot.Rp:N0} RP";
    }
}
