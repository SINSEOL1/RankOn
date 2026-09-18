using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RankOn.Dialogs;

public partial class SessionRpDialog : Window
{
    private readonly int _currentRp;

    public SessionRpDialog(int currentRp, int? startRp)
    {
        InitializeComponent();
        App.LocalizationService.ApplyTo(this);

        _currentRp = currentRp;
        RpTextBox.Text = (startRp ?? currentRp).ToString();
        RpTextBox.SelectAll();
        UpdatePreview();

        RpTextBox.TextChanged += (_, _) => UpdatePreview();
    }

    public int StartRp { get; private set; }

    private void RpTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RpTextBox.Text, out var value) || value < 0)
        {
            return;
        }

        StartRp = value;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void UpdatePreview()
    {
        if (!int.TryParse(RpTextBox.Text, out var value))
        {
            PreviewText.Text = "";
            return;
        }

        var delta = _currentRp - value;
        PreviewText.Text = App.LocalizationService.CurrentRpDelta(delta);
    }
}
