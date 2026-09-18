using System.Windows;
using RankOn.Services;

namespace RankOn;

public partial class App : System.Windows.Application
{
    public static AppSettingsService SettingsService { get; } = new();
    public static OverlayStateService OverlayStateService { get; } = new();
    public static SessionService SessionService { get; } = new();
    public static ThemeService ThemeService { get; } = new();
    public static LocalBroadcastServer BroadcastServer { get; } = new(OverlayStateService, 19872);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = await SettingsService.LoadAsync();
        ThemeService.Apply(settings.Theme);

        try
        {
            await BroadcastServer.StartAsync();
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
        await BroadcastServer.StopAsync();
        base.OnExit(e);
    }
}
