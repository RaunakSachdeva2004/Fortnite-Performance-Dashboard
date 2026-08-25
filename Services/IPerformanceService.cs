using FortniteDashboard.Models;
using FortniteDashboard.ViewModels;

namespace FortniteDashboard.Services
{
    public interface IPerformanceService
    {
        decimal CalculatePerformanceScore(Stats stats);
        string GetPerformanceLevel(decimal score);
        Task<RecentPerformanceSummary> GetRecentFormSummaryAsync(int playerId, CancellationToken cancellationToken = default);
        Task<AnalyticsViewModel> GetAnalyticsOverviewAsync(int playerId, string period = "30d", CancellationToken cancellationToken = default);
    }
}
