using System.Net.Http;
using System.Net.Http.Json;
using RankOn.Models;

namespace RankOn.Services;

public sealed class RankApiService : IDisposable
{
    private const string BaseUrl = "https://er-companion-proxy.vercel.app/rankon/";
    private readonly HttpClient _httpClient;

    public RankApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(12)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RankOn/0.1.0");
    }

    public async Task<(PlayerProfile Profile, RankSnapshot Snapshot)> ResolveAsync(
        string nickname,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<RankApiResponse>(
            $"profile?nickname={Uri.EscapeDataString(nickname.Trim())}",
            cancellationToken);

        var profile = new PlayerProfile
        {
            Nickname = response.Nickname,
            Uid = response.Uid
        };

        return (profile, response.ToSnapshot());
    }

    public async Task<RankSnapshot> GetRankAsync(
        string uid,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<RankApiResponse>(
            $"rank?uid={Uri.EscapeDataString(uid)}",
            cancellationToken);

        return response.ToSnapshot();
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"RankOn API request failed ({(int)response.StatusCode}): {body}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<T>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("RankOn API returned an empty response.");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
