using FortniteDashboard.Models;

namespace FortniteDashboard.ViewModels
{
    public class DashboardViewModel
    {
        public string PlayerName { get; set; } = string.Empty;
        public string FortniteUsername { get; set; } = string.Empty;
        public string? Team { get; set; }

        public Stats? Stats { get; set; }
        public List<Recommendation> Recommendations { get; set; } = new();

        public bool HasStats => Stats is not null;

        // For Chart.js: label/value pairs, most recent last.
        // Populate however your DashboardController wants to shape history;
        // currently a single current-snapshot point since Stats is 1:1 with Player.
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartWinRateSeries { get; set; } = new();
        public List<decimal> ChartKDSeries { get; set; } = new();
    }
}
