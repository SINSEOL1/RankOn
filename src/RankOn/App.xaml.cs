using System.Windows;
using RankOn.Services;

namespace RankOn;

public partial class App : System.Windows.Application
{
    public static AppSettingsService SettingsService { get; } = new();
    public static OverlayStateService OverlayStateService { get; } = new();
    public static SessionService SessionService { get; } = new(SettingsService);
    public static ThemeService ThemeService { get; } = new();
    public static ProfileService ProfileService { get; } = new();
    public static RankApiService RankApiService { get; } = new();
    public static RankPollingService RankPollingService { get; } = new(
        ProfileService,
        RankApiService,
        SessionService,
        OverlayStateService,
        SettingsService);
    public static LocalBroadcastServer BroadcastServer { get; } = new(OverlayStateService, 19872);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = await SettingsService.LoadAsync();
        SessionService.Initialize(settings);
        ThemeService.Apply(settings.Theme);

        try
        {
            await BroadcastServer.StartAsync();
        }
        catch
        {
        }

        try
        {
            await RankPollingService.StartAsync();
        }
        catch
        {
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await RankPollingService.DisposeAsync();
        RankApiService.Dispose();
        await BroadcastServer.StopAsync();
        base.OnExit(e);
    }
}
