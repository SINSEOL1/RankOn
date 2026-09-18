using System.Windows;
using RankOn.Services;

namespace RankOn;

public partial class App : System.Windows.Application
{
    public static AppSettingsService SettingsService { get; } = new();
    public static OverlayStateService OverlayStateService { get; } = new();
    public static SessionService SessionService { get; } = new(SettingsService);
    public static ThemeService ThemeService { get; } = new();
    public static LocalizationService LocalizationService { get; } = new();
    public static ProfileService ProfileService { get; } = new();
    public static RankApiService RankApiService { get; } = new();
    public static RankPollingService RankPollingService { get; } = new(
        ProfileService,
        RankApiService,
        SessionService,
        OverlayStateService,
        SettingsService);
    public static LocalBroadcastServer BroadcastServer { get; } = new(OverlayStateService, 19872);
    public static OverlayWindowService PcOverlayService { get; } = new(OverlayStateService, SettingsService);
    public static StartupService StartupService { get; } = new();
    public static UpdateService UpdateService { get; } = new();
    public static TrayService TrayService { get; } = new();

    public static bool IsExiting { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = await SettingsService.LoadAsync();
        SessionService.Initialize(settings);
        ThemeService.Apply(settings.Theme);
        LocalizationService.Apply(settings.Language);
        PcOverlayService.Initialize();

        if (settings.BroadcastEnabled)
        {
            try
            {
                await BroadcastServer.StartAsync();
            }
            catch
            {
            }
        }

        try
        {
            await RankPollingService.StartAsync();
        }
        catch
        {
        }

        if (settings.PcOverlayEnabled)
        {
            PcOverlayService.Show();
        }

        var window = new MainWindow();
        MainWindow = window;
        TrayService.Initialize(window);

        if (!settings.StartMinimizedToTray)
        {
            window.Show();
        }

        if (settings.AutoCheckUpdates)
        {
            _ = CheckUpdatesSilentlyAsync(window);
        }
    }

    private static async Task CheckUpdatesSilentlyAsync(Window owner)
    {
        try
        {
            var update = await UpdateService.CheckAsync();
            if (update is null)
            {
                return;
            }

            owner.Dispatcher.Invoke(() =>
            {
                var result = System.Windows.MessageBox.Show(
                    $"새 버전 {update.Version}이 있습니다. 다운로드 페이지를 열까요?",
                    "랭크온 업데이트",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    UpdateService.Open(update);
                }
            });
        }
        catch
        {
        }
    }

    public static void ExitApplication()
    {
        IsExiting = true;
        Current.Shutdown();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        TrayService.Dispose();
        PcOverlayService.Dispose();
        await RankPollingService.DisposeAsync();
        RankApiService.Dispose();
        UpdateService.Dispose();
        await BroadcastServer.StopAsync();
        base.OnExit(e);
    }
}
