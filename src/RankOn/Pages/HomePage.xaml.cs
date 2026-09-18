using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RankOn.Dialogs;
using RankOn.Services;

namespace RankOn.Pages;

public partial class HomePage : UserControl
{
    private bool _subscribed;
    private DateTimeOffset _nextManualRefresh = DateTimeOffset.MinValue;

    public HomePage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_subscribed)
        {
            App.RankPollingService.Changed += RankPollingService_Changed;
            App.PcOverlayService.Changed += OverlayService_Changed;
            _subscribed = true;
        }

        UpdateView();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_subscribed)
        {
            App.RankPollingService.Changed -= RankPollingService_Changed;
            App.PcOverlayService.Changed -= OverlayService_Changed;
            _subscribed = false;
        }
    }

    private void RankPollingService_Changed(object? sender, EventArgs e) => Dispatcher.Invoke(UpdateView);
    private void OverlayService_Changed(object? sender, EventArgs e) => Dispatcher.Invoke(UpdateView);

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (DateTimeOffset.Now < _nextManualRefresh) return;

        _nextManualRefresh = DateTimeOffset.Now.AddSeconds(10);
        UpdateView();
        await App.RankPollingService.RefreshAsync();
        UpdateView();

        var wait = _nextManualRefresh - DateTimeOffset.Now;
        if (wait > TimeSpan.Zero) await Task.Delay(wait);
        if (_subscribed) UpdateView();
    }

    private async void NewSession_Click(object sender, RoutedEventArgs e)
    {
        var snapshot = App.RankPollingService.Current;
        if (snapshot is null) return;

        await App.SessionService.StartFromCurrentAsync(snapshot.Rp);
        App.RankPollingService.RebuildOverlayState();
        UpdateView();
    }

    private async void ManualSession_Click(object sender, RoutedEventArgs e)
    {
        var snapshot = App.RankPollingService.Current;
        if (snapshot is null) return;

        var dialog = new SessionRpDialog(snapshot.Rp, App.SessionService.Current.StartRp)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true) return;

        await App.SessionService.StartFromManualAsync(dialog.StartRp);
        App.RankPollingService.RebuildOverlayState();
        UpdateView();
    }

    private void UpdateView()
    {
        var service = App.RankPollingService;
        var snapshot = service.Current;
        var profile = service.CurrentProfile;
        var cooldown = _nextManualRefresh - DateTimeOffset.Now;
        var canRefresh = profile is not null && !service.IsRefreshing && cooldown <= TimeSpan.Zero;

        RefreshButton.IsEnabled = canRefresh;
        RefreshButton.Content = service.IsRefreshing
            ? "불러오는 중..."
            : cooldown > TimeSpan.Zero
                ? $"{Math.Ceiling(cooldown.TotalSeconds):0}초 후"
                : "새로고침";

        SetStatus(PcOverlayStatusText, App.PcOverlayService.IsVisible);
        SetStatus(BroadcastStatusText, App.BroadcastServer.IsRunning);

        if (snapshot is null)
        {
            NicknameText.Text = profile?.Nickname ?? "프로필을 등록해주세요";
            TierText.Text = profile is null
                ? "프로필 페이지에서 닉네임을 등록하면 시작됩니다."
                : "랭크 정보를 불러오는 중입니다.";
            RpText.Text = "— RP";
            RankText.Text = "—";
            SeasonText.Text = "";
            CutText.Text = "";
            SessionDeltaText.Text = "0 RP";
            SessionStartText.Text = App.SessionService.Current.StartRp is int start ? $"시작 RP {start:N0}" : "시작 RP —";
            StatusText.Text = service.LastError ?? "프로필을 등록해주세요.";
            return;
        }

        NicknameText.Text = snapshot.Nickname;
        TierText.Text = snapshot.TierDisplayName;
        RpText.Text = $"{snapshot.Rp:N0} RP";
        RankText.Text = snapshot.Rank > 0 ? $"#{snapshot.Rank:N0}" : "순위 없음";
        RankBadgeText.Text = GetBadgeText(snapshot.TierKey);
        SeasonText.Text = FormatSeasonRemaining(snapshot.SeasonEnd);
        CutText.Text = RankTargetDisplayService.GetText(snapshot, App.SettingsService.Current.TargetRpDisplayMode);

        var delta = App.SessionService.GetDelta(snapshot.Rp);
        SessionDeltaText.Text = $"{(delta > 0 ? "+" : "")}{delta:N0} RP";
        SessionDeltaText.Foreground = delta >= 0
            ? (Brush)FindResource("PositiveBrush")
            : (Brush)FindResource("NegativeBrush");

        SessionStartText.Text = App.SessionService.Current.StartRp is int startRp ? $"시작 RP {startRp:N0}" : "시작 RP —";
        StatusText.Text = service.LastError ?? "자동 갱신 60초";
    }

    private void SetStatus(TextBlock text, bool enabled)
    {
        text.Text = enabled ? "ON" : "OFF";
        text.Foreground = enabled
            ? (Brush)FindResource("PositiveBrush")
            : (Brush)FindResource("MutedTextBrush");
    }

    private static string GetBadgeText(string tierKey) => tierKey switch
    {
        "eternity" => "ET", "demigod" => "DG", "mythril" => "MI",
        "meteorite" => "ME", "diamond" => "DI", "platinum" => "PL",
        "gold" => "GO", "silver" => "SI", "bronze" => "BR", _ => "IR"
    };

    private static string FormatSeasonRemaining(DateTimeOffset? seasonEnd)
    {
        if (seasonEnd is null) return "";
        var remaining = seasonEnd.Value - DateTimeOffset.Now;
        if (remaining <= TimeSpan.Zero) return "시즌 종료";
        return remaining.TotalDays >= 1
            ? $"시즌 종료까지 {(int)remaining.TotalDays}일 {remaining.Hours}시간"
            : $"시즌 종료까지 {remaining.Hours}시간 {remaining.Minutes}분";
    }
}
