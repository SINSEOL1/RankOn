using RankOn.Models;

namespace RankOn.Services;

public sealed class SessionService
{
    private readonly AppSettingsService _settingsService;

    public SessionService(AppSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public SessionState Current { get; } = new();

    public void Initialize(AppSettings settings)
    {
        Current.StartRp = settings.SessionStartRp;
        Current.StartedAt = settings.SessionStartedAt;
    }

    public async Task StartFromCurrentAsync(int currentRp)
    {
        await SaveAsync(currentRp);
    }

    public async Task StartFromManualAsync(int startRp)
    {
        await SaveAsync(Math.Max(0, startRp));
    }

    public int GetDelta(int currentRp)
    {
        return Current.StartRp is int startRp ? currentRp - startRp : 0;
    }

    public async Task ResetAsync()
    {
        Current.StartRp = null;
        Current.StartedAt = null;

        var settings = _settingsService.Current;
        settings.SessionStartRp = null;
        settings.SessionStartedAt = null;
        await _settingsService.SaveAsync(settings);
    }

    private async Task SaveAsync(int startRp)
    {
        Current.StartRp = startRp;
        Current.StartedAt = DateTimeOffset.Now;

        var settings = _settingsService.Current;
        settings.SessionStartRp = Current.StartRp;
        settings.SessionStartedAt = Current.StartedAt;
        await _settingsService.SaveAsync(settings);
    }
}
