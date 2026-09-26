using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace RankOn.Services;

public sealed class UpdateService : IDisposable
{
    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public UpdateService()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("RankOn-Updater");
    }

    public async Task<UpdateInfo?> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _client.GetAsync(
            "https://api.github.com/repos/SINSEOL1/RankOn/releases/latest",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(
            cancellationToken: cancellationToken);

        if (release is null || string.IsNullOrWhiteSpace(release.TagName))
        {
            return null;
        }

        var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
        var raw = release.TagName.Trim().TrimStart('v', 'V');

        if (!Version.TryParse(raw, out var latest) || latest <= current)
        {
            return null;
        }

        var installer = release.Assets
            .FirstOrDefault(asset =>
                asset.Name.StartsWith("RankOn-Setup-v", StringComparison.OrdinalIgnoreCase) &&
                asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl));

        if (installer is null)
        {
            return null;
        }

        return new UpdateInfo(
            release.TagName,
            installer.Name,
            installer.BrowserDownloadUrl,
            installer.Digest);
    }

    public async Task<bool> DownloadAndInstallAsync(
        UpdateInfo update,
        CancellationToken cancellationToken = default)
    {
        var updateDirectory = Path.Combine(
            Path.GetTempPath(),
            "RankOn",
            "Updates",
            SanitizeVersion(update.Version));

        Directory.CreateDirectory(updateDirectory);

        var installerPath = Path.Combine(updateDirectory, update.FileName);
        var partialPath = installerPath + ".download";

        try
        {
            if (File.Exists(partialPath))
            {
                File.Delete(partialPath);
            }

            using (var response = await _client.GetAsync(
                       update.DownloadUrl,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var destination = new FileStream(
                    partialPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

                await source.CopyToAsync(destination, cancellationToken);
            }

            if (!await VerifyDigestAsync(partialPath, update.Digest, cancellationToken))
            {
                File.Delete(partialPath);
                return false;
            }

            File.Move(partialPath, installerPath, overwrite: true);

            var executablePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                executablePath = Path.Combine(AppContext.BaseDirectory, "RankOn.exe");
            }

            LaunchUpdateHelper(installerPath, executablePath);
            return true;
        }
        catch
        {
            try
            {
                if (File.Exists(partialPath))
                {
                    File.Delete(partialPath);
                }
            }
            catch
            {
            }

            return false;
        }
    }

    private static async Task<bool> VerifyDigestAsync(
        string path,
        string? digest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(digest) ||
            !digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var expected = digest["sha256:".Length..].Trim();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        var actualBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        var actual = Convert.ToHexString(actualBytes);

        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static void LaunchUpdateHelper(string installerPath, string executablePath)
    {
        var scriptPath = Path.Combine(
            Path.GetTempPath(),
            "RankOn",
            "Updates",
            $"apply-{Guid.NewGuid():N}.ps1");

        Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);

        var installer = EscapePowerShellLiteral(installerPath);
        var executable = EscapePowerShellLiteral(executablePath);
        var script = EscapePowerShellLiteral(scriptPath);
        var currentPid = Environment.ProcessId;

        var contents = $$"""
$ErrorActionPreference = 'SilentlyContinue'
$installer = '{{installer}}'
$executable = '{{executable}}'
$script = '{{script}}'

Wait-Process -Id {{currentPid}} -ErrorAction SilentlyContinue

$process = Start-Process -FilePath $installer -ArgumentList @(
    '/VERYSILENT',
    '/SUPPRESSMSGBOXES',
    '/NORESTART',
    '/SP-',
    '/CLOSEAPPLICATIONS'
) -Wait -PassThru

if ($process.ExitCode -eq 0 -and (Test-Path -LiteralPath $executable)) {
    Start-Process -FilePath $executable
}

Remove-Item -LiteralPath $installer -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $script -Force -ErrorAction SilentlyContinue
""";

        File.WriteAllText(scriptPath, contents, new UTF8Encoding(false));

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        });
    }

    private static string EscapePowerShellLiteral(string value)
    {
        return value.Replace("'", "''");
    }

    private static string SanitizeVersion(string version)
    {
        return string.Concat(version.Where(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_'));
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";

        [JsonPropertyName("digest")]
        public string? Digest { get; set; }
    }
}

public sealed record UpdateInfo(
    string Version,
    string FileName,
    string DownloadUrl,
    string? Digest);
