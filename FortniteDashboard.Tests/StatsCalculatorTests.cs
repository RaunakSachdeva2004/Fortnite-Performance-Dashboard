using FortniteDashboard.Services;
using Xunit;

namespace FortniteDashboard.Tests;

public class StatsCalculatorTests
{
    [Theory]
    [InlineData(20, 10, 10)]   // 20 matches, 10 wins -> 10 deaths
    [InlineData(5, 5, 0)]      // undefeated -> 0 deaths, never negative
    [InlineData(0, 0, 0)]      // no matches yet
    public void ComputeDeaths_NeverGoesNegative(int matchesPlayed, int wins, int expectedDeaths)
    {
        var deaths = StatsCalculator.ComputeDeaths(matchesPlayed, wins);
        Assert.Equal(expectedDeaths, deaths);
    }

    [Fact]
    public void ComputeKDRatio_DividesEliminationsByDeaths()
    {
        var kd = StatsCalculator.ComputeKDRatio(eliminations: 40, deaths: 20);
        Assert.Equal(2.00m, kd);
    }

    [Fact]
    public void ComputeKDRatio_WhenDeathsIsZero_FallsBackToEliminations_NoDivideByZero()
    {
        var kd = StatsCalculator.ComputeKDRatio(eliminations: 15, deaths: 0);
        Assert.Equal(15m, kd);
    }

    [Fact]
    public void ComputeWinRate_CalculatesPercentageCorrectly()
    {
        var winRate = StatsCalculator.ComputeWinRate(wins: 5, matchesPlayed: 20);
        Assert.Equal(25.00m, winRate);
    }

    [Fact]
    public void ComputeWinRate_WhenNoMatchesPlayed_ReturnsZero_NoDivideByZero()
    {
        var winRate = StatsCalculator.ComputeWinRate(wins: 0, matchesPlayed: 0);
        Assert.Equal(0m, winRate);
    }
}
