using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RankOn.Models;

namespace RankOn.Windows;

public partial class OverlayWindow
{
    private bool _recentMatchesSubscribed;

    private void OverlayWindow_RecentMatchesLoaded(object sender, RoutedEventArgs e)
    {
        if (!_recentMatchesSubscribed)
        {
            App.OverlayStateService.Changed += OverlayStateService_RecentMatchesChanged;
            _recentMatchesSubscribed = true;
        }

        ApplyRecentMatchesState(App.OverlayStateService.Get());
    }

    private void OverlayWindow_RecentMatchesUnloaded(object sender, RoutedEventArgs e)
    {
        if (!_recentMatchesSubscribed)
        {
            return;
        }

        App.OverlayStateService.Changed -= OverlayStateService_RecentMatchesChanged;
        _recentMatchesSubscribed = false;
    }

    private void OverlayStateService_RecentMatchesChanged(object? sender, OverlayState state)
    {
        Dispatcher.Invoke(() => ApplyRecentMatchesState(state));
    }

    private void ApplyRecentMatchesState(OverlayState state)
    {
        RecentMatchesPanel.Visibility =
            state.HasData && state.ShowRecentMatches
                ? Visibility.Visible
                : Visibility.Collapsed;

        var vertical = string.Equals(state.Preset, "Vertical", StringComparison.OrdinalIgnoreCase);
        RecentMatchesPanel.HorizontalAlignment = vertical
            ? HorizontalAlignment.Center
            : HorizontalAlignment.Left;
        RecentMatchesTitle.TextAlignment = vertical
            ? TextAlignment.Center
            : TextAlignment.Left;

        RecentMatchesGrid.Children.Clear();

        for (var index = 0; index < 10; index++)
        {
            var rank = index < state.RecentMatches.Count
                ? state.RecentMatches[index].Rank
                : (int?)null;

            RecentMatchesGrid.Children.Add(CreateRecentMatchTile(rank));
        }
    }

    private static Border CreateRecentMatchTile(int? rank)
    {
        return new Border
        {
            Width = 25,
            Height = 25,
            Margin = new Thickness(2),
            CornerRadius = new CornerRadius(4),
            Background = GetRecentMatchBrush(rank),
            Child = new TextBlock
            {
                Text = rank?.ToString() ?? "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            }
        };
    }

    private static Brush GetRecentMatchBrush(int? rank)
    {
        return rank switch
        {
            1 => new SolidColorBrush(Color.FromRgb(201, 147, 38)),
            2 => new SolidColorBrush(Color.FromRgb(143, 154, 168)),
            3 => new SolidColorBrush(Color.FromRgb(168, 106, 67)),
            >= 4 and <= 8 => new SolidColorBrush(Color.FromRgb(58, 66, 77)),
            _ => new SolidColorBrush(Color.FromArgb(90, 58, 66, 77))
        };
    }
}
