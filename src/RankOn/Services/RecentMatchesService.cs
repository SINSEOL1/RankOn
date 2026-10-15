using RankOn.Models;

namespace RankOn.Services;

public sealed class RecentMatchesService
{
    private readonly object _sync = new();
    private readonly List<RecentMatchResult> _latestMatches = new();
    private readonly List<RecentMatchResult> _latestManualMatches = new();
    private readonly List<RecentMatchResult> _sessionMatches = new();
    private readonly HashSet<long> _knownGameIds = new();
    private bool _initialized;
    private long _nextManualGameId = -1;

    public void ResetForProfile()
    {
        lock (_sync)
        {
            _latestMatches.Clear();
            _latestManualMatches.Clear();
            _sessionMatches.Clear();
            _knownGameIds.Clear();
            _initialized = false;
            _nextManualGameId = -1;
        }
    }

    public void Update(IEnumerable<RecentMatchResult> matches)
    {
        var normalized = Normalize(matches);

        lock (_sync)
        {
            _latestMatches.Clear();
            _latestMatches.AddRange(normalized);

            if (!_initialized)
            {
                _knownGameIds.Clear();
                _knownGameIds.UnionWith(normalized.Select(match => match.GameId));
                _sessionMatches.Clear();
                _initialized = true;
                return;
            }

            var newlyCompleted = normalized
                .Where(match => !_knownGameIds.Contains(match.GameId))
                .OrderBy(match => match.GameId)
                .ToArray();

            foreach (var match in newlyCompleted)
            {
                _sessionMatches.Add(Clone(match));
            }

            TrimToTen(_sessionMatches);
            _knownGameIds.UnionWith(normalized.Select(match => match.GameId));
        }
    }

    public void AddManual(int placement, string mode)
    {
        if (placement is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(placement),
                "등수는 1~8 사이여야 합니다.");
        }

        lock (_sync)
        {
            var match = new RecentMatchResult
            {
                GameId = _nextManualGameId--,
                Rank = placement
            };

            if (string.Equals(
                mode,
                "Latest10",
                StringComparison.OrdinalIgnoreCase))
            {
                _latestManualMatches.Add(match);
                TrimToTen(_latestManualMatches);
            }
            else
            {
                _sessionMatches.Add(match);
                TrimToTen(_sessionMatches);
            }
        }
    }

    public void ResetSession()
    {
        lock (_sync)
        {
            _sessionMatches.Clear();
            _knownGameIds.UnionWith(_latestMatches.Select(match => match.GameId));
        }
    }

    public IReadOnlyList<RecentMatchResult> GetDisplay(string mode)
    {
        lock (_sync)
        {
            IEnumerable<RecentMatchResult> source;

            if (string.Equals(
                mode,
                "Latest10",
                StringComparison.OrdinalIgnoreCase))
            {
                source = _latestMatches
                    .Take(10)
                    .Reverse()
                    .Concat(_latestManualMatches)
                    .TakeLast(10);
            }
            else
            {
                source = _sessionMatches;
            }

            return source.Select(Clone).ToArray();
        }
    }

    private static void TrimToTen(List<RecentMatchResult> matches)
    {
        if (matches.Count > 10)
        {
            matches.RemoveRange(0, matches.Count - 10);
        }
    }

    private static List<RecentMatchResult> Normalize(IEnumerable<RecentMatchResult> matches)
    {
        return matches
            .Where(match => match.GameId > 0 && match.Rank is >= 1 and <= 8)
            .GroupBy(match => match.GameId)
            .Select(group => group.First())
            .OrderByDescending(match => match.GameId)
            .Take(30)
            .Select(Clone)
            .ToList();
    }

    private static RecentMatchResult Clone(RecentMatchResult match)
    {
        return new RecentMatchResult
        {
            GameId = match.GameId,
            Rank = match.Rank
        };
    }
}
