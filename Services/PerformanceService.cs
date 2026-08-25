using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly ApplicationDbContext _db;

        public PerformanceService(ApplicationDbContext db)
        {
            _db = db;
        }

        public decimal CalculatePerformanceScore(Stats stats)
        {
            if (stats == null || stats.MatchesPlayed == 0) return 0m;

            // 1. K/D Score (capped at 5.0 K/D = 100 pts)
            decimal kdScore = Math.Min(100m, (stats.KDRatio / 5.0m) * 100m);

            // 2. Win Rate Score (capped at 30% = 100 pts)
            decimal winRateScore = Math.Min(100m, (stats.WinRate / 30.0m) * 100m);

            // 3. Placement Score (1st place = 100, 50th = 0)
            decimal placementVal = stats.AvgPlacement > 0 ? stats.AvgPlacement : (50m - Math.Min(49m, stats.WinRate * 0.5m));
            decimal placementScore = Math.Max(0m, 100m - ((placementVal - 1m) / 49m) * 100m);

            // 4. Accuracy Score (capped at 40% = 100 pts)
            decimal score;
            if (stats.Accuracy > 0)
            {
                decimal accuracyScore = Math.Min(100m, (stats.Accuracy / 40.0m) * 100m);
                score = (winRateScore * 0.30m) + (kdScore * 0.30m) + (placementScore * 0.20m) + (accuracyScore * 0.20m);
            }
            else
            {
                // Accuracy unavailable: reweight to 35% WinRate, 35% KD, 30% Placement
                score = (winRateScore * 0.35m) + (kdScore * 0.35m) + (placementScore * 0.30m);
            }

            return Math.Round(score, 1);
        }

        public string GetPerformanceLevel(decimal score)
        {
            if (score >= 85m) return "Excellent";
            if (score >= 70m) return "Strong";
            if (score >= 55m) return "Developing";
            return "Needs Improvement";
        }

        public async Task<RecentPerformanceSummary> GetRecentFormSummaryAsync(int playerId, CancellationToken cancellationToken = default)
        {
            var summary = new RecentPerformanceSummary();

            var recentMatches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.PlayerId == playerId)
                .OrderByDescending(m => m.PlayedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            if (recentMatches.Count == 0) return summary;

            // Split into recent 5 vs previous 5
            var recent5 = recentMatches.Take(5).ToList();
            var previous5 = recentMatches.Skip(5).Take(5).ToList();

            summary.MatchesAnalyzed = recentMatches.Count;

            int recentKills = recent5.Sum(m => m.Eliminations);
            int recentDeaths = Math.Max(1, recent5.Sum(m => m.Deaths));
            summary.RecentKD = Math.Round((decimal)recentKills / recentDeaths, 2);

            int recentWins = recent5.Count(m => m.IsWin);
            summary.RecentWinRate = Math.Round((decimal)recentWins / recent5.Count * 100m, 1);
            summary.RecentAvgPlacement = Math.Round((decimal)recent5.Average(m => m.Placement), 1);
            summary.RecentAccuracy = Math.Round((decimal)recent5.Average(m => (double)m.Accuracy), 1);

            if (previous5.Count > 0)
            {
                int prevKills = previous5.Sum(m => m.Eliminations);
                int prevDeaths = Math.Max(1, previous5.Sum(m => m.Deaths));
                summary.PreviousKD = Math.Round((decimal)prevKills / prevDeaths, 2);

                int prevWins = previous5.Count(m => m.IsWin);
                summary.PreviousWinRate = Math.Round((decimal)prevWins / previous5.Count * 100m, 1);
                summary.PreviousAvgPlacement = Math.Round((decimal)previous5.Average(m => m.Placement), 1);
                summary.PreviousAccuracy = Math.Round((decimal)previous5.Average(m => (double)m.Accuracy), 1);
            }
            else
            {
                summary.PreviousKD = summary.RecentKD;
                summary.PreviousWinRate = summary.RecentWinRate;
                summary.PreviousAvgPlacement = summary.RecentAvgPlacement;
                summary.PreviousAccuracy = summary.RecentAccuracy;
            }

            summary.KDTrend = summary.RecentKD > summary.PreviousKD ? "up" : (summary.RecentKD < summary.PreviousKD ? "down" : "equal");
            summary.WinRateTrend = summary.RecentWinRate > summary.PreviousWinRate ? "up" : (summary.RecentWinRate < summary.PreviousWinRate ? "down" : "equal");
            summary.PlacementTrend = summary.RecentAvgPlacement < summary.PreviousAvgPlacement ? "up" : (summary.RecentAvgPlacement > summary.PreviousAvgPlacement ? "down" : "equal"); // Lower placement number = better rank
            summary.AccuracyTrend = summary.RecentAccuracy > summary.PreviousAccuracy ? "up" : (summary.RecentAccuracy < summary.PreviousAccuracy ? "down" : "equal");

            return summary;
        }

        public async Task<AnalyticsViewModel> GetAnalyticsOverviewAsync(int playerId, string period = "30d", CancellationToken cancellationToken = default)
        {
            var player = await _db.Players.AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);

            var vm = new AnalyticsViewModel
            {
                PlayerName = player?.User?.Name ?? "Player",
                FortniteUsername = player?.FortniteUsername ?? "Gamer",
                SelectedPeriod = period
            };

            DateTime startDate = period switch
            {
                "7d" => DateTime.UtcNow.AddDays(-7),
                "30d" => DateTime.UtcNow.AddDays(-30),
                "90d" => DateTime.UtcNow.AddDays(-90),
                _ => DateTime.MinValue
            };

            var matches = await _db.Matches.AsNoTracking()
                .Where(m => m.PlayerId == playerId && m.PlayedAt >= startDate)
                .OrderBy(m => m.PlayedAt)
                .ToListAsync(cancellationToken);

            var statsHistory = await _db.Stats.AsNoTracking()
                .Where(s => s.PlayerId == playerId && s.RecordedAt >= startDate)
                .OrderBy(s => s.RecordedAt)
                .ToListAsync(cancellationToken);

            if (statsHistory.Count > 0)
            {
                var latestStats = statsHistory.Last();
                vm.OverallPerformanceScore = latestStats.PerformanceScore > 0 ? latestStats.PerformanceScore : CalculatePerformanceScore(latestStats);
                vm.PreviousPerformanceScore = statsHistory.Count > 1 ? statsHistory[^2].PerformanceScore : vm.OverallPerformanceScore;
                vm.PerformanceLevel = GetPerformanceLevel(vm.OverallPerformanceScore);

                vm.ChartLabels = statsHistory.Select(s => s.RecordedAt.ToLocalTime().ToString("MMM d")).ToList();
                vm.KDSeries = statsHistory.Select(s => s.KDRatio).ToList();
                vm.WinRateSeries = statsHistory.Select(s => s.WinRate).ToList();
                vm.PlacementSeries = statsHistory.Select(s => s.AvgPlacement > 0 ? s.AvgPlacement : 10m).ToList();
                vm.DamageSeries = statsHistory.Select(s => s.AvgDamage).ToList();
            }

            if (matches.Count > 0)
            {
                int totalKills = matches.Sum(m => m.Eliminations);
                int totalDeaths = Math.Max(1, matches.Sum(m => m.Deaths));
                int totalWins = matches.Count(m => m.IsWin);

                vm.Combat = new CombatStatsSummary
                {
                    TotalEliminations = totalKills,
                    TotalDeaths = totalDeaths,
                    KDRatio = Math.Round((decimal)totalKills / totalDeaths, 2),
                    AvgDamagePerMatch = Math.Round((decimal)matches.Average(m => m.DamageDealt), 1),
                    Accuracy = Math.Round((decimal)matches.Average(m => (double)m.Accuracy), 1)
                };

                vm.Survival = new SurvivalStatsSummary
                {
                    TotalWins = totalWins,
                    WinRate = Math.Round((decimal)totalWins / matches.Count * 100m, 1),
                    AvgPlacement = Math.Round((decimal)matches.Average(m => m.Placement), 1),
                    Top3Placements = matches.Count(m => m.Placement <= 3),
                    Top10Placements = matches.Count(m => m.Placement <= 10)
                };

                vm.Consistency = new ConsistencyStatsSummary
                {
                    MatchesPlayed = matches.Count,
                    PerformanceScore = vm.OverallPerformanceScore,
                    PerformanceLevel = vm.PerformanceLevel,
                    BestPlacement = matches.Min(m => m.Placement),
                    WorstPlacement = matches.Max(m => m.Placement),
                    ScoreVariance = Math.Round((decimal)matches.Average(m => Math.Abs(m.Placement - matches.Average(m2 => m2.Placement))), 1)
                };
            }

            return vm;
        }
    }
}
