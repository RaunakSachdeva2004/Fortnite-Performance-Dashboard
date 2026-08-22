namespace FortniteDashboard.Services;

/// <summary>
/// Pure, dependency-free KPI math used by <see cref="StatsService"/>.
/// Kept separate (no EF Core, no HttpClient, no DI) specifically so it can be
/// unit tested directly — see FortniteDashboard.Tests/StatsCalculatorTests.cs.
/// </summary>
public static class StatsCalculator
{
    /// <summary>
    /// FortniteAPI.io does not report a "deaths" figure directly. By standard
    /// battle-royale convention (used by most stat trackers), every match that
    /// didn't end in a win counts as one death — you were eliminated or the
    /// match simply ended without a Victory Royale.
    /// </summary>
    public static int ComputeDeaths(int matchesPlayed, int wins)
    {
        var deaths = matchesPlayed - wins;
        return deaths < 0 ? 0 : deaths;
    }

    /// <summary>
    /// K/D ratio = eliminations / deaths, guarded against division by zero.
    /// When deaths is 0 (e.g. a 100% win rate, or no matches yet), we fall
    /// back to eliminations itself rather than throwing or returning
    /// infinity/NaN.
    /// </summary>
    public static decimal ComputeKDRatio(int eliminations, int deaths)
    {
        if (deaths <= 0)
        {
            return Math.Round((decimal)eliminations, 2);
        }

        return Math.Round(eliminations / (decimal)deaths, 2);
    }

    /// <summary>
    /// Win rate as a percentage (0-100), guarded against division by zero.
    /// </summary>
    public static decimal ComputeWinRate(int wins, int matchesPlayed)
    {
        if (matchesPlayed <= 0)
        {
            return 0m;
        }

        return Math.Round(wins / (decimal)matchesPlayed * 100m, 2);
    }
}
