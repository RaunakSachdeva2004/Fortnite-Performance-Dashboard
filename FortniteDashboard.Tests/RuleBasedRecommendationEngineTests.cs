using FortniteDashboard.Models;
using FortniteDashboard.Services;
using Xunit;

namespace FortniteDashboard.Tests;

public class RuleBasedRecommendationEngineTests
{
    private readonly RuleBasedRecommendationEngine _engine = new();

    private static Stats StatsWith(int matchesPlayed, decimal kd, decimal winRate, decimal accuracy = 0)
        => new()
        {
            MatchesPlayed = matchesPlayed,
            KDRatio = kd,
            WinRate = winRate,
            Accuracy = accuracy
        };

    [Fact]
    public void LowMatchVolume_SuggestsPlayingMoreMatchesOnly()
    {
        var stats = StatsWith(matchesPlayed: 3, kd: 0.5m, winRate: 0m);

        var result = _engine.GenerateRecommendations(stats);

        var tip = Assert.Single(result);
        Assert.Contains("more matches", tip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LowKDRatio_SuggestsPositioning()
    {
        var stats = StatsWith(matchesPlayed: 20, kd: 0.8m, winRate: 8m);

        var result = _engine.GenerateRecommendations(stats);

        Assert.Contains(result, r => r.Contains("positioning", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void HighKDRatio_GivesPositiveReinforcement()
    {
        var stats = StatsWith(matchesPlayed: 20, kd: 3.5m, winRate: 8m);

        var result = _engine.GenerateRecommendations(stats);

        Assert.Contains(result, r => r.Contains("Excellent K/D", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LowWinRate_SuggestsSaferRotations()
    {
        var stats = StatsWith(matchesPlayed: 20, kd: 1.5m, winRate: 2m);

        var result = _engine.GenerateRecommendations(stats);

        Assert.Contains(result, r => r.Contains("win rate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ZeroAccuracy_IsTreatedAsUnknown_AndDoesNotFireAccuracyRule()
    {
        // Accuracy defaults to 0 when the Fortnite stats API doesn't supply it. Confirms
        // the fix for the earlier unit-mismatch bug (0.15/0.30 fractions vs a
        // 0-100 percentage) doesn't cause a false "low accuracy" warning here.
        var stats = StatsWith(matchesPlayed: 20, kd: 1.5m, winRate: 8m, accuracy: 0m);

        var result = _engine.GenerateRecommendations(stats);

        Assert.DoesNotContain(result, r => r.Contains("accuracy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RealisticLowAccuracy_FiresAccuracyRule_AtPercentageScale()
    {
        // 12 means 12%, not 0.12 -- this is the bug that was fixed.
        var stats = StatsWith(matchesPlayed: 20, kd: 1.5m, winRate: 8m, accuracy: 12m);

        var result = _engine.GenerateRecommendations(stats);

        Assert.Contains(result, r => r.Contains("accuracy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AllMetricsAverage_ReturnsFallbackEncouragement()
    {
        var stats = StatsWith(matchesPlayed: 20, kd: 1.5m, winRate: 8m, accuracy: 0m);

        var result = _engine.GenerateRecommendations(stats);

        var tip = Assert.Single(result);
        Assert.Contains("consistently", tip, StringComparison.OrdinalIgnoreCase);
    }
}
