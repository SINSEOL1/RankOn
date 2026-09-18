using RankOn.Models;

namespace RankOn.Services;

public sealed class OverlayStateService
{
    private readonly object _sync = new();
    private OverlayState _state = OverlayState.Empty;

    public event EventHandler<OverlayState>? Changed;

    public OverlayState Get()
    {
        lock (_sync)
        {
            return _state;
        }
    }

    public void Set(OverlayState state)
    {
        lock (_sync)
        {
            _state = state;
        }

        Changed?.Invoke(this, state);
    }
}
