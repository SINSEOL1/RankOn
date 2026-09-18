using System.Windows;
using System.Windows.Controls;

namespace RankOn.Pages;

public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void CopyDiscord_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText("sinseol");
    }
}
