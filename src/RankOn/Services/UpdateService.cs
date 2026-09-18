using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RankOn.Services;

public sealed class UpdateService : IDisposable
{
    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(8)
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

        return new UpdateInfo(release.TagName, release.HtmlUrl);
    }

    public void Open(UpdateInfo update)
    {
        if (string.IsNullOrWhiteSpace(update.Url))
        {
            return;
        }

        Process.Start(new ProcessStartInfo(update.Url)
        {
            UseShellExecute = true
        });
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = "";
    }
}

public sealed record UpdateInfo(string Version, string Url);
