using RankOn.Models;

namespace RankOn.Services;

public sealed class SessionService
{
    public SessionState Current { get; } = new();

    public void StartFromCurrent(int currentRp)
    {
        Current.StartRp = currentRp;
        Current.StartedAt = DateTimeOffset.Now;
    }

    public void StartFromManual(int startRp)
    {
        Current.StartRp = Math.Max(0, startRp);
        Current.StartedAt = DateTimeOffset.Now;
    }

    public int GetDelta(int currentRp)
    {
        return Current.StartRp is int startRp ? currentRp - startRp : 0;
    }

    public void Reset()
    {
        Current.StartRp = null;
        Current.StartedAt = null;
    }
}
