using System.Text.Json.Serialization;

namespace RankOn.Models;

public sealed class RecentMatchResult
{
    [JsonPropertyName("gameId")]
    public long GameId { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }
}

public sealed class RecentMatchesResponse
{
    [JsonPropertyName("matches")]
    public List<RecentMatchResult> Matches { get; set; } = new();
}
