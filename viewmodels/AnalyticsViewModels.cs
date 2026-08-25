using FortniteDashboard.Models;

namespace FortniteDashboard.ViewModels
{
    public class RecentPerformanceSummary
    {
        public decimal RecentKD { get; set; }
        public decimal PreviousKD { get; set; }
        public string KDTrend { get; set; } = "equal"; // "up", "down", "equal"

        public decimal RecentWinRate { get; set; }
        public decimal PreviousWinRate { get; set; }
        public string WinRateTrend { get; set; } = "equal";

        public decimal RecentAvgPlacement { get; set; }
        public decimal PreviousAvgPlacement { get; set; }
        public string PlacementTrend { get; set; } = "equal";

        public decimal RecentAccuracy { get; set; }
        public decimal PreviousAccuracy { get; set; }
        public string AccuracyTrend { get; set; } = "equal";

        public int MatchesAnalyzed { get; set; }
    }

    public class CombatStatsSummary
    {
        public int TotalEliminations { get; set; }
        public int TotalDeaths { get; set; }
        public decimal KDRatio { get; set; }
        public decimal AvgDamagePerMatch { get; set; }
        public decimal Accuracy { get; set; }
    }

    public class SurvivalStatsSummary
    {
        public int TotalWins { get; set; }
        public decimal WinRate { get; set; }
        public decimal AvgPlacement { get; set; }
        public int Top3Placements { get; set; }
        public int Top10Placements { get; set; }
    }

    public class ConsistencyStatsSummary
    {
        public int MatchesPlayed { get; set; }
        public decimal PerformanceScore { get; set; }
        public string PerformanceLevel { get; set; } = "Developing";
        public int BestPlacement { get; set; } = 100;
        public int WorstPlacement { get; set; } = 100;
        public decimal ScoreVariance { get; set; }
    }

    public class AnalyticsViewModel
    {
        public string PlayerName { get; set; } = string.Empty;
        public string FortniteUsername { get; set; } = string.Empty;
        public string SelectedPeriod { get; set; } = "30d"; // "7d", "30d", "90d", "all"

        public decimal OverallPerformanceScore { get; set; }
        public decimal PreviousPerformanceScore { get; set; }
        public decimal ScoreChange => OverallPerformanceScore - PreviousPerformanceScore;
        public string PerformanceLevel { get; set; } = "Developing";

        public CombatStatsSummary Combat { get; set; } = new();
        public SurvivalStatsSummary Survival { get; set; } = new();
        public ConsistencyStatsSummary Consistency { get; set; } = new();

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> KDSeries { get; set; } = new();
        public List<decimal> WinRateSeries { get; set; } = new();
        public List<decimal> PlacementSeries { get; set; } = new();
        public List<decimal> DamageSeries { get; set; } = new();
    }
}
