using RankOn.Models;

namespace RankOn.Services;

public sealed class RecentMatchesService
{
    private readonly object _sync = new();
    private readonly List<RecentMatchResult> _latestMatches = new();
    private readonly List<RecentMatchResult> _sessionMatches = new();
    private readonly HashSet<long> _knownGameIds = new();
    private bool _initialized;

    public void ResetForProfile()
    {
        lock (_sync)
        {
            _latestMatches.Clear();
            _sessionMatches.Clear();
            _knownGameIds.Clear();
            _initialized = false;
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

            if (_sessionMatches.Count > 10)
            {
                _sessionMatches.RemoveRange(0, _sessionMatches.Count - 10);
            }

            _knownGameIds.UnionWith(normalized.Select(match => match.GameId));
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
            IEnumerable<RecentMatchResult> source = string.Equals(
                mode,
                "Latest10",
                StringComparison.OrdinalIgnoreCase)
                ? _latestMatches.Take(10).Reverse()
                : _sessionMatches;

            return source.Select(Clone).ToArray();
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
