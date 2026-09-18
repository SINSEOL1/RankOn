using RankOn.Models;

namespace RankOn.Services;

public static class RankTargetDisplayService
{
    public static string GetText(RankSnapshot snapshot, string mode)
    {
        return mode switch
        {
            "Hidden" => "",
            "Demigod" => FormatCut("데미갓", snapshot.DemigodCutRp, snapshot.Rp),
            "Eternity" => FormatCut("이터니티", snapshot.EternityCutRp, snapshot.Rp),
            "NextTier" => FormatNextTier(snapshot),
            _ => FormatAuto(snapshot)
        };
    }

    private static string FormatAuto(RankSnapshot snapshot)
    {
        if (snapshot.TierKey == "eternity")
        {
            return "";
        }

        return FormatNextTier(snapshot);
    }

    private static string FormatNextTier(RankSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.NextTierName) ||
            snapshot.NextTierRemainingRp is not int remaining)
        {
            return "";
        }

        return $"{snapshot.NextTierName}까지 {remaining:N0} RP";
    }

    private static string FormatCut(string name, int? targetRp, int currentRp)
    {
        if (targetRp is not int target)
        {
            return "";
        }

        return $"{name}까지 {Math.Max(0, target - currentRp):N0} RP";
    }
}
