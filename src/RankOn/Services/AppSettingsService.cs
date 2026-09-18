using System.Globalization;
using System.IO;
using System.Text.Json;
using RankOn.Models;

namespace RankOn.Services;

public sealed class AppSettingsService
{
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RankOn");

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    public AppSettings Current { get; private set; } = new();

    public async Task<AppSettings> LoadAsync()
    {
        Directory.CreateDirectory(_directory);

        if (!File.Exists(SettingsPath))
        {
            Current.Language = ResolveInitialLanguage();
            await SaveAsync(Current);
            return Current;
        }

        try
        {
            var json = await File.ReadAllTextAsync(SettingsPath);
            Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            Current = new AppSettings();
        }

        return Current;
    }

    private static string ResolveInitialLanguage()
    {
        var name = CultureInfo.CurrentUICulture.Name;

        if (name.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) return "ko-KR";
        if (name.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return "ja-JP";
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return "zh-CN";
        return "en-US";
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(_directory);
        Current = settings;

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(SettingsPath, json);
    }
}
