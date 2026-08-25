using FortniteDashboard.Models;

namespace FortniteDashboard.ViewModels
{
    public class DashboardViewModel
    {
        public string PlayerName { get; set; } = string.Empty;
        public string FortniteUsername { get; set; } = string.Empty;
        public string? Team { get; set; }
        public string PreferredGameMode { get; set; } = "Solo";

        public Stats? Stats { get; set; }
        public decimal PerformanceScore { get; set; }
        public string PerformanceLevel { get; set; } = "Developing";

        public RecentPerformanceSummary RecentForm { get; set; } = new();
        public List<Recommendation> Recommendations { get; set; } = new();

        public bool HasStats => Stats is not null;

        public List<Stats> History { get; set; } = new();
        public List<Match> RecentMatches { get; set; } = new();

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartWinRateSeries { get; set; } = new();
        public List<decimal> ChartKDSeries { get; set; } = new();
    }
}
