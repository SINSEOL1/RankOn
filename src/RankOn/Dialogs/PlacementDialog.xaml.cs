using System.Windows;
using System.Windows.Controls;

namespace RankOn.Dialogs;

public partial class PlacementDialog : Window
{
    public PlacementDialog()
    {
        InitializeComponent();
        App.LocalizationService.ApplyTo(this);
    }

    public int Placement { get; private set; }

    private void Placement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string value } ||
            !int.TryParse(value, out var placement) ||
            placement is < 1 or > 8)
        {
            return;
        }

        Placement = placement;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
