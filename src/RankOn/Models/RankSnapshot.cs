namespace RankOn.Models;

public sealed class RankSnapshot
{
    public string Nickname { get; init; } = "";
    public string Uid { get; init; } = "";
    public int SeasonId { get; init; }
    public string SeasonName { get; init; } = "";
    public DateTimeOffset? SeasonEnd { get; init; }
    public int Rp { get; init; }
    public int Rank { get; init; }
    public int ServerRank { get; init; }
    public string TierKey { get; init; } = "";
    public string TierName { get; init; } = "";
    public int? Division { get; init; }
    public int? DemigodCutRp { get; init; }
    public int? EternityCutRp { get; init; }
    public string? NextTierName { get; init; }
    public int? NextTierRp { get; init; }
    public int? NextTierRemainingRp { get; init; }
    public string? NextCutTierName { get; init; }
    public int? NextCutRp { get; init; }
    public int? NextCutRemainingRp { get; init; }

    public string TierDisplayName =>
        Division is > 0
            ? $"{TierName} {Division}"
            : TierName;
}
