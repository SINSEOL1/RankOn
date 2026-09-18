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
}
