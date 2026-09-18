using RankOn.Models;

namespace RankOn.Services;

public sealed class RankPollingService : IAsyncDisposable
{
    private readonly ProfileService _profileService;
    private readonly RankApiService _apiService;
    private readonly SessionService _sessionService;
    private readonly OverlayStateService _overlayStateService;
    private readonly AppSettingsService _settingsService;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private CancellationTokenSource? _loopCancellation;
    private Task? _loopTask;

    public RankPollingService(
        ProfileService profileService,
        RankApiService apiService,
        SessionService sessionService,
        OverlayStateService overlayStateService,
        AppSettingsService settingsService)
    {
        _profileService = profileService;
        _apiService = apiService;
        _sessionService = sessionService;
        _overlayStateService = overlayStateService;
        _settingsService = settingsService;
    }

    public PlayerProfile? CurrentProfile { get; private set; }
    public RankSnapshot? Current { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }
    public bool IsRefreshing { get; private set; }

    public event EventHandler? Changed;

    public async Task StartAsync()
    {
        CurrentProfile = await _profileService.LoadAsync();

        if (CurrentProfile is not null)
        {
            await RefreshAsync();
        }

        _loopCancellation = new CancellationTokenSource();
        _loopTask = RunLoopAsync(_loopCancellation.Token);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetProfileByNicknameAsync(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            throw new ArgumentException("닉네임을 입력해주세요.", nameof(nickname));
        }

        await _refreshGate.WaitAsync();

        try
        {
            IsRefreshing = true;
            Changed?.Invoke(this, EventArgs.Empty);

            var result = await _apiService.ResolveAsync(nickname);
            CurrentProfile = result.Profile;
            await _profileService.SaveAsync(result.Profile);
            await _sessionService.StartFromCurrentAsync(result.Snapshot.Rp);
            ApplySnapshot(result.Snapshot);
            LastError = null;
        }
        finally
        {
            IsRefreshing = false;
            _refreshGate.Release();
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task RefreshAsync()
    {
        var profile = CurrentProfile;

        if (profile is null)
        {
            return;
        }

        if (!await _refreshGate.WaitAsync(0))
        {
            return;
        }

        try
        {
            IsRefreshing = true;
            Changed?.Invoke(this, EventArgs.Empty);

            var snapshot = await _apiService.GetRankAsync(profile.Uid);

            if (_sessionService.Current.StartRp is null)
            {
                await _sessionService.StartFromCurrentAsync(snapshot.Rp);
            }

            ApplySnapshot(snapshot);
            LastError = null;
        }
        catch (Exception ex)
        {
            LastError = GetErrorMessage(ex);
        }
        finally
        {
            IsRefreshing = false;
            _refreshGate.Release();
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RebuildOverlayState()
    {
        if (Current is not null)
        {
            ApplyOverlayState(Current);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySnapshot(RankSnapshot snapshot)
    {
        Current = snapshot;
        LastUpdatedAt = DateTimeOffset.Now;
        ApplyOverlayState(snapshot);
    }

    private void ApplyOverlayState(RankSnapshot snapshot)
    {
        _overlayStateService.Set(new OverlayState(
            true,
            snapshot.Nickname,
            snapshot.TierDisplayName,
            snapshot.Rp,
            snapshot.Rank,
            _sessionService.GetDelta(snapshot.Rp),
            FormatSeasonRemaining(snapshot.SeasonEnd)));
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var seconds = Math.Max(30, _settingsService.Current.RankRefreshSeconds);
                await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
                await RefreshAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static string FormatSeasonRemaining(DateTimeOffset? seasonEnd)
    {
        if (seasonEnd is null)
        {
            return "";
        }

        var remaining = seasonEnd.Value - DateTimeOffset.Now;

        if (remaining <= TimeSpan.Zero)
        {
            return "시즌 종료";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"시즌 종료까지 {(int)remaining.TotalDays}일 {remaining.Hours}시간";
        }

        return $"시즌 종료까지 {Math.Max(0, remaining.Hours)}시간 {remaining.Minutes}분";
    }

    private static string GetErrorMessage(Exception exception)
    {
        if (exception is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound })
        {
            return "현재 시즌 랭크 정보를 찾지 못했습니다.";
        }

        if (exception is TaskCanceledException)
        {
            return "랭크 정보 요청 시간이 초과되었습니다.";
        }

        return "랭크 정보를 불러오지 못했습니다.";
    }

    public async ValueTask DisposeAsync()
    {
        if (_loopCancellation is not null)
        {
            await _loopCancellation.CancelAsync();
        }

        if (_loopTask is not null)
        {
            try
            {
                await _loopTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _loopCancellation?.Dispose();
        _refreshGate.Dispose();
    }
}
