using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RankOn.Services;

public sealed record UpdateInfo(
    Version Version,
    string VersionText,
    string InstallerUrl);

public sealed class UpdateService : IDisposable
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/SINSEOL1/RankOn/releases/latest";

    private readonly HttpClient _httpClient = new();

    public UpdateService()
    {
        var current = Assembly.GetExecutingAssembly().GetName().Version;
        var versionText = current is null
            ? "1.0.0"
            : $"{current.Major}.{current.Minor}.{Math.Max(0, current.Build)}";

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"RankOn/{versionText}");

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<UpdateInfo?> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            LatestReleaseApi,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        var release = await JsonSerializer.DeserializeAsync<ReleasePayload>(
            stream,
            cancellationToken: cancellationToken);

        if (release is null ||
            string.IsNullOrWhiteSpace(release.TagName) ||
            release.Draft ||
            release.Prerelease)
        {
            return null;
        }

        var versionText = release.TagName.Trim().TrimStart('v', 'V');

        if (!Version.TryParse(versionText, out var latestVersion))
            return null;

        var currentVersion =
            Assembly.GetExecutingAssembly().GetName().Version
            ?? new Version(1, 0, 0, 0);

        if (latestVersion <= currentVersion)
            return null;

        var installer = release.Assets.FirstOrDefault(asset =>
            asset.Name.StartsWith(
                "RankOn-Setup-v",
                StringComparison.OrdinalIgnoreCase) &&
            asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (installer is null ||
            string.IsNullOrWhiteSpace(installer.BrowserDownloadUrl))
        {
            return null;
        }

        return new UpdateInfo(
            latestVersion,
            versionText,
            installer.BrowserDownloadUrl);
    }

    public async Task<string> DownloadInstallerAsync(
        UpdateInfo update,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "RankOn",
            "updates",
            update.VersionText);

        Directory.CreateDirectory(directory);

        var destination = Path.Combine(
            directory,
            $"RankOn-Setup-v{update.VersionText}.exe");

        using var response = await _httpClient.GetAsync(
            update.InstallerUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var totalLength = response.Content.Headers.ContentLength;

        await using var input = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        await using var output = new FileStream(
            destination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        var buffer = new byte[81920];
        long totalRead = 0;

        while (true)
        {
            var read = await input.ReadAsync(
                buffer.AsMemory(0, buffer.Length),
                cancellationToken);

            if (read == 0)
                break;

            await output.WriteAsync(
                buffer.AsMemory(0, read),
                cancellationToken);

            totalRead += read;

            if (totalLength is > 0)
            {
                progress?.Report(
                    Math.Clamp(totalRead / (double)totalLength.Value, 0, 1));
            }
        }

        progress?.Report(1);
        return destination;
    }

    public bool LaunchInstaller(string installerPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments =
                    "/SILENT /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /NORESTART",
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose() => _httpClient.Dispose();

    private sealed class ReleasePayload
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("assets")]
        public List<ReleaseAsset> Assets { get; set; } = [];
    }

    private sealed class ReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}
