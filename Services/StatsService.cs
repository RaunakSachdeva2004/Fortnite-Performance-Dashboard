using FortniteDashboard.Data;
using FortniteDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Services
{
    public class StatsService : IStatsService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFortniteApiClient _fortniteApiClient;
        private readonly IRecommendationEngine _recommendationEngine;
        private readonly IPerformanceService _performanceService;
        private readonly ILogger<StatsService> _logger;

        public StatsService(
            ApplicationDbContext db,
            IFortniteApiClient fortniteApiClient,
            IRecommendationEngine recommendationEngine,
            IPerformanceService performanceService,
            ILogger<StatsService> logger)
        {
            _db = db;
            _fortniteApiClient = fortniteApiClient;
            _recommendationEngine = recommendationEngine;
            _performanceService = performanceService;
            _logger = logger;
        }

        public async Task<Stats?> GetStatsForPlayerAsync(int playerId, CancellationToken cancellationToken = default)
        {
            return await _db.Stats
                .AsNoTracking()
                .Where(s => s.PlayerId == playerId)
                .OrderByDescending(s => s.RecordedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Stats>> GetStatsHistoryForPlayerAsync(int playerId, int take = 10, CancellationToken cancellationToken = default)
        {
            var recent = await _db.Stats
                .AsNoTracking()
                .Where(s => s.PlayerId == playerId)
                .OrderByDescending(s => s.RecordedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            recent.Reverse();
            return recent;
        }

        public async Task<List<Recommendation>> GetRecommendationsForPlayerAsync(int playerId, int take = 5, CancellationToken cancellationToken = default)
        {
            return await _db.Recommendations
                .AsNoTracking()
                .Where(r => r.PlayerId == playerId)
                .OrderByDescending(r => r.CreatedDate)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<Stats>> SyncPlayerStatsAsync(int playerId, string epicUsername, CancellationToken cancellationToken = default)
        {
            var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);
            if (player is null)
            {
                return Result<Stats>.Failure($"Player {playerId} not found.");
            }

            var apiResult = await _fortniteApiClient.GetPlayerStatsAsync(epicUsername, cancellationToken);
            if (!apiResult.IsSuccess || apiResult.Value is null)
            {
                _logger.LogWarning("Fortnite API sync failed for {Username}: {Error}", epicUsername, apiResult.ErrorMessage);
                return Result<Stats>.Failure(apiResult.ErrorMessage ?? "Fortnite API returned no data.");
            }

            var snapshot = MapApiResponseToSnapshot(playerId, epicUsername, apiResult.Value);
            snapshot.PerformanceScore = _performanceService.CalculatePerformanceScore(snapshot);

            _db.Stats.Add(snapshot);

            if (!string.Equals(player.FortniteUsername, epicUsername, StringComparison.OrdinalIgnoreCase))
            {
                player.FortniteUsername = epicUsername;
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Generate structured recommendations
            var newRecommendations = _recommendationEngine.GenerateStructuredRecommendations(snapshot);
            foreach (var rec in newRecommendations)
            {
                rec.PlayerId = playerId;
            }

            _db.Recommendations.AddRange(newRecommendations);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Synced stats for player {PlayerId} ({Username}). Performance Score: {Score}", playerId, epicUsername, snapshot.PerformanceScore);

            return Result<Stats>.Success(snapshot);
        }

        private static Stats MapApiResponseToSnapshot(int playerId, string epicUsername, FortniteOverallStats apiData)
        {
            int eliminations = apiData.Kills;
            int wins = apiData.Wins;
            int matchesPlayed = apiData.Matches;

            int deaths = StatsCalculator.ComputeDeaths(matchesPlayed, wins);
            decimal kdRatio = StatsCalculator.ComputeKDRatio(eliminations, deaths);
            decimal winRate = StatsCalculator.ComputeWinRate(wins, matchesPlayed);

            return new Stats
            {
                PlayerId = playerId,
                SyncedUsername = epicUsername,
                Eliminations = eliminations,
                Wins = wins,
                MatchesPlayed = matchesPlayed,
                Deaths = deaths,
                KDRatio = kdRatio,
                WinRate = winRate,
                Accuracy = 0m,
                AvgPlacement = Math.Round(Math.Max(1m, 50m - (winRate * 0.5m)), 1),
                RecordedAt = DateTime.UtcNow
            };
        }
    }
}
