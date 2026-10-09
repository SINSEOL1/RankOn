namespace RankOn.Models;

public sealed record OverlayState(
    bool HasData,
    string Nickname,
    string Tier,
    string TierKey,
    int Rp,
    int Rank,
    int SessionDelta,
    string SeasonRemaining,
    string TargetRpText,
    IReadOnlyList<RecentMatchResult> RecentMatches,
    string Preset,
    bool BackgroundEnabled,
    double BackgroundOpacity,
    double CornerRadius,
    double FontScale,
    bool ShowNickname,
    bool ShowTier,
    bool ShowTierIcon,
    bool ShowRp,
    bool ShowRank,
    bool ShowSession,
    bool ShowSeason,
    bool ShowTarget,
    bool ShowRecentMatches)
{
    public static OverlayState Empty { get; } = new(
        false, "", "", "", 0, 0, 0, "", "", Array.Empty<RecentMatchResult>(), "Standard",
        true, 0.88, 14, 1.0, true, true, true, true, true, true, true, true, true);
}
