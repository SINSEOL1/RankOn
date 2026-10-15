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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateLayout();

        var workArea = SystemParameters.WorkArea;
        var desiredLeft = workArea.Left + (workArea.Width - ActualWidth) / 2;
        var desiredTop = workArea.Top + (workArea.Height - ActualHeight) / 2;

        if (Owner is not null)
        {
            desiredLeft = Owner.Left + (Owner.ActualWidth - ActualWidth) / 2;
            desiredTop = Owner.Top + (Owner.ActualHeight - ActualHeight) / 2;
        }

        Left = Math.Clamp(
            desiredLeft,
            workArea.Left + 8,
            Math.Max(workArea.Left + 8, workArea.Right - ActualWidth - 8));

        Top = Math.Clamp(
            desiredTop,
            workArea.Top + 8,
            Math.Max(workArea.Top + 8, workArea.Bottom - ActualHeight - 8));
    }

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
