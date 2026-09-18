namespace RankOn.Models;

public sealed record OverlayState(
    bool HasData,
    string Nickname,
    string Tier,
    int Rp,
    int Rank,
    int SessionDelta,
    string SeasonRemaining)
{
    public static OverlayState Empty { get; } = new(false, "", "", 0, 0, 0, "");
}
