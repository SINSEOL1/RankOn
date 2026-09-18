using RankOn.Models;
using RankOn.Windows;

namespace RankOn.Services;

public sealed class OverlayWindowService : IDisposable
{
    private readonly OverlayStateService _stateService;
    private readonly AppSettingsService _settingsService;
    private OverlayWindow? _window;
    private bool _positionMode;

    public OverlayWindowService(
        OverlayStateService stateService,
        AppSettingsService settingsService)
    {
        _stateService = stateService;
        _settingsService = settingsService;
    }

    public bool IsVisible => _window?.IsVisible == true;
    public bool IsPositionMode => _positionMode;

    public event EventHandler? Changed;

    public void Initialize()
    {
        _stateService.Changed += StateService_Changed;
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        var settings = _settingsService.Current;
        settings.PcOverlayEnabled = enabled;
        await _settingsService.SaveAsync(settings);

        if (enabled)
        {
            Show();
        }
        else
        {
            Hide();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task ToggleAsync()
    {
        await SetEnabledAsync(!IsVisible);
    }

    public void Show()
    {
        EnsureWindow();
        ApplySettings();
        _window!.SetState(_stateService.Get());

        if (!_window.IsVisible)
        {
            _window.Show();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Hide()
    {
        _positionMode = false;
        _window?.SetPositionMode(false);
        _window?.Hide();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void ApplySettings()
    {
        if (_window is null)
        {
            return;
        }

        _window.ApplyWindowSettings(_settingsService.Current);
        _window.SetState(_stateService.Get());
        _window.SetPositionMode(_positionMode);
    }

    public async Task SetPositionModeAsync(bool enabled)
    {
        _positionMode = enabled;

        if (enabled)
        {
            Show();
        }

        _window?.SetPositionMode(enabled);

        if (!enabled && _window is not null)
        {
            await SavePositionAsync();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task ResetPositionAsync()
    {
        var settings = _settingsService.Current;
        settings.PcOverlayX = 80;
        settings.PcOverlayY = 80;
        await _settingsService.SaveAsync(settings);

        if (_window is not null)
        {
            _window.Left = 80;
            _window.Top = 80;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureWindow()
    {
        if (_window is not null)
        {
            return;
        }

        _window = new OverlayWindow();
        _window.PositionCommitted += async (_, _) => await SavePositionAsync();

        var settings = _settingsService.Current;

        if (settings.PcOverlayX is null || settings.PcOverlayY is null)
        {
            _window.Left = 80;
            _window.Top = 80;
        }
    }

    private async Task SavePositionAsync()
    {
        if (_window is null)
        {
            return;
        }

        var settings = _settingsService.Current;
        settings.PcOverlayX = _window.Left;
        settings.PcOverlayY = _window.Top;
        await _settingsService.SaveAsync(settings);
    }

    private void StateService_Changed(object? sender, OverlayState state)
    {
        if (_window is null)
        {
            return;
        }

        _window.Dispatcher.Invoke(() => _window.SetState(state));
    }

    public void Dispose()
    {
        _stateService.Changed -= StateService_Changed;

        if (_window is not null)
        {
            _window.Close();
            _window = null;
        }
    }
}
