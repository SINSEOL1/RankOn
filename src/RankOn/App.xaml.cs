using System.Diagnostics;
using System.Threading;
using System.Windows;
using RankOn.Services;

namespace RankOn;

public partial class App : System.Windows.Application
{
    private static Mutex? _instanceMutex;

    public static AppSettingsService SettingsService { get; } = new();
    public static OverlayStateService OverlayStateService { get; } = new();
    public static SessionService SessionService { get; } = new(SettingsService);
    public static RecentMatchesService RecentMatchesService { get; } = new();
    public static ThemeService ThemeService { get; } = new();
    public static LocalizationService LocalizationService { get; } = new();
    public static ProfileService ProfileService { get; } = new();
    public static RankApiService RankApiService { get; } = new();
    public static RankPollingService RankPollingService { get; } = new(
        ProfileService,
        RankApiService,
        SessionService,
        RecentMatchesService,
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

        _instanceMutex = new Mutex(true, @"Local\SINSEOL.RankOn", out var createdNew);
        var otherProcess = Process.GetProcessesByName("RankOn")
            .Any(process => process.Id != Environment.ProcessId);

        if (!createdNew || otherProcess)
        {
            System.Windows.MessageBox.Show(
                "랭크온이 이미 실행 중입니다. 트레이에 실행 중인 랭크온을 종료한 뒤 다시 실행해주세요.",
                "랭크온",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        var settings = await SettingsService.LoadAsync();
        SessionService.Initialize(settings);
        ThemeService.Apply(settings.Theme);
        LocalizationService.Apply(settings.Language);
        _ = TierIconService.WarmupAsync();
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

        window.Show();

        if (settings.StartMinimizedToTray)
        {
            window.Hide();
        }

        if (settings.AutoCheckUpdates)
        {
            _ = CheckAndApplyUpdateAsync();
        }
    }

    private static async Task CheckAndApplyUpdateAsync()
    {
        try
        {
            var update = await UpdateService.CheckAsync();
            if (update is null)
                return;

            var installerPath = await UpdateService.DownloadInstallerAsync(update);

            if (!UpdateService.LaunchInstaller(installerPath))
                return;

            await Task.Delay(300);
            Current.Dispatcher.Invoke(ExitApplication);
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

        try
        {
            _instanceMutex?.ReleaseMutex();
        }
        catch
        {
        }

        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
