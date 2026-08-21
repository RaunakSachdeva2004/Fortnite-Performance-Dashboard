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
        private readonly ILogger<StatsService> _logger;

        public StatsService(
            ApplicationDbContext db,
            IFortniteApiClient fortniteApiClient,
            IRecommendationEngine recommendationEngine,
            ILogger<StatsService> logger)
        {
            _db = db;
            _fortniteApiClient = fortniteApiClient;
            _recommendationEngine = recommendationEngine;
            _logger = logger;
        }

        public async Task<Stats?> GetStatsForPlayerAsync(int playerId)
        {
            return await _db.Stats
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PlayerId == playerId);
        }

        public async Task<List<Recommendation>> GetRecommendationsForPlayerAsync(int playerId, int take = 5)
        {
            return await _db.Recommendations
                .AsNoTracking()
                .Where(r => r.PlayerId == playerId)
                .OrderByDescending(r => r.CreatedDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Stats> SyncPlayerStatsAsync(int playerId, string epicUsername)
        {
            var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == playerId)
                ?? throw new InvalidOperationException($"Player {playerId} not found.");

            var apiResult = await _fortniteApiClient.GetPlayerStatsAsync(epicUsername)
                ?? throw new InvalidOperationException($"Fortnite API returned no data for '{epicUsername}'.");

            // ---- Map API response onto the Stats entity (insert or update) ----
            var stats = await _db.Stats.FirstOrDefaultAsync(s => s.PlayerId == playerId);
            if (stats is null)
            {
                stats = new Stats { PlayerId = playerId };
                _db.Stats.Add(stats);
            }

            stats.Eliminations = apiResult.Eliminations;
            stats.Wins = apiResult.Wins;
            stats.MatchesPlayed = apiResult.MatchesPlayed;
            stats.Accuracy = apiResult.Accuracy;
            stats.KDRatio = apiResult.KDRatio;
            stats.LastUpdated = DateTime.UtcNow;

            // Keep the Player's stored Epic username in sync in case the user
            // synced under a different name than what's on file.
            if (!string.Equals(player.FortniteUsername, epicUsername, StringComparison.OrdinalIgnoreCase))
            {
                player.FortniteUsername = epicUsername;
            }

            await _db.SaveChangesAsync();

            // WinRate is a DB-computed column — reload to get the value SQL Server calculated.
            await _db.Entry(stats).ReloadAsync();

            // ---- Generate & persist fresh coaching recommendations ----
            var recommendationTexts = _recommendationEngine.GenerateRecommendations(stats);

            if (recommendationTexts.Count > 0)
            {
                var newRecommendations = recommendationTexts.Select(text => new Recommendation
                {
                    PlayerId = playerId,
                    RecommendationText = text,
                    CreatedDate = DateTime.UtcNow
                });

                _db.Recommendations.AddRange(newRecommendations);
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Synced stats for player {PlayerId} ({Username}).", playerId, epicUsername);

            return stats;
        }
    }
}
