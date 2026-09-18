using System.Text.Json.Serialization;

namespace RankOn.Models;

public sealed class RankApiResponse
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = "";

    [JsonPropertyName("uid")]
    public string Uid { get; set; } = "";

    [JsonPropertyName("season")]
    public RankSeasonResponse Season { get; set; } = new();

    [JsonPropertyName("rp")]
    public int Rp { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("serverRank")]
    public int ServerRank { get; set; }

    [JsonPropertyName("tier")]
    public RankTierResponse Tier { get; set; } = new();

    [JsonPropertyName("cuts")]
    public RankCutsResponse Cuts { get; set; } = new();

    [JsonPropertyName("nextCut")]
    public RankNextCutResponse? NextCut { get; set; }

    public RankSnapshot ToSnapshot()
    {
        DateTimeOffset? end = null;

        if (DateTimeOffset.TryParse(Season.SeasonEnd, out var parsedEnd))
        {
            end = parsedEnd;
        }

        return new RankSnapshot
        {
            Nickname = Nickname,
            Uid = Uid,
            SeasonId = Season.SeasonId,
            SeasonName = Season.SeasonName,
            SeasonEnd = end,
            Rp = Rp,
            Rank = Rank,
            ServerRank = ServerRank,
            TierKey = Tier.Key,
            TierName = Tier.NameKo,
            Division = Tier.Division,
            DemigodCutRp = Cuts.DemigodRp,
            EternityCutRp = Cuts.EternityRp,
            NextCutTierName = NextCut?.TierNameKo,
            NextCutRp = NextCut?.Rp,
            NextCutRemainingRp = NextCut?.RemainingRp
        };
    }
}

public sealed class RankSeasonResponse
{
    [JsonPropertyName("seasonId")]
    public int SeasonId { get; set; }

    [JsonPropertyName("seasonName")]
    public string SeasonName { get; set; } = "";

    [JsonPropertyName("seasonEnd")]
    public string? SeasonEnd { get; set; }
}

public sealed class RankTierResponse
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("nameKo")]
    public string NameKo { get; set; } = "";

    [JsonPropertyName("division")]
    public int? Division { get; set; }
}

public sealed class RankCutsResponse
{
    [JsonPropertyName("demigodRp")]
    public int? DemigodRp { get; set; }

    [JsonPropertyName("eternityRp")]
    public int? EternityRp { get; set; }
}

public sealed class RankNextCutResponse
{
    [JsonPropertyName("tierNameKo")]
    public string TierNameKo { get; set; } = "";

    [JsonPropertyName("rp")]
    public int Rp { get; set; }

    [JsonPropertyName("remainingRp")]
    public int RemainingRp { get; set; }
}
