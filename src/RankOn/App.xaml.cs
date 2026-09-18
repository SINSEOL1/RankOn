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
                var message = App.LocalizationService.Current switch
                {
                    "en-US" => $"New version {update.Version} is available. Open the download page?",
                    "ja-JP" => $"新しいバージョン {update.Version} があります。ダウンロードページを開きますか？",
                    "zh-CN" => $"发现新版本 {update.Version}。是否打开下载页面？",
                    _ => $"새 버전 {update.Version}이 있습니다. 다운로드 페이지를 열까요?"
                };

                var title = App.LocalizationService.Current switch
                {
                    "en-US" => "RankOn Update",
                    "ja-JP" => "RankOn アップデート",
                    "zh-CN" => "RankOn 更新",
                    _ => "랭크온 업데이트"
                };

                var result = System.Windows.MessageBox.Show(
                    message,
                    title,
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
