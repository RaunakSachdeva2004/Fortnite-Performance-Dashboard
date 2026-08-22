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

            // Return oldest -> newest so charts read left-to-right correctly.
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

            // ---- Call the external API and unwrap its typed Result ----
            // (Previously this code read fields like apiResult.Eliminations
            // directly off the Result<T> wrapper, which doesn't exist there —
            // it would not compile. The actual player data lives one level
            // down, in apiResult.Value.)
            var apiResult = await _fortniteApiClient.GetPlayerStatsAsync(epicUsername);
            if (!apiResult.IsSuccess || apiResult.Value is null)
            {
                _logger.LogWarning("Fortnite API sync failed for {Username}: {Error}", epicUsername, apiResult.ErrorMessage);
                return Result<Stats>.Failure(apiResult.ErrorMessage ?? "Fortnite API returned no data.");
            }

            var snapshot = MapApiResponseToSnapshot(playerId, epicUsername, apiResult.Value);

            // Always INSERT a new snapshot rather than updating an existing
            // row — that's what makes historical trend charts possible.
            _db.Stats.Add(snapshot);

            // Keep the Player's stored Epic username in sync in case the user
            // synced under a different name than what's on file.
            if (!string.Equals(player.FortniteUsername, epicUsername, StringComparison.OrdinalIgnoreCase))
            {
                player.FortniteUsername = epicUsername;
            }

            await _db.SaveChangesAsync(cancellationToken);

            // ---- Generate & persist fresh coaching recommendations ----
            var recommendationTexts = _recommendationEngine.GenerateRecommendations(snapshot);

            if (recommendationTexts.Count > 0)
            {
                var newRecommendations = recommendationTexts.Select(text => new Recommendation
                {
                    PlayerId = playerId,
                    RecommendationText = text,
                    CreatedDate = DateTime.UtcNow
                });

                _db.Recommendations.AddRange(newRecommendations);
                await _db.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Synced stats for player {PlayerId} ({Username}).", playerId, epicUsername);

            return Result<Stats>.Success(snapshot);
        }

        /// <summary>
        /// Maps a raw Fortnite-API.com response onto a new Stats snapshot.
        ///
        /// ASSUMPTION (isolated here, see also Models/FortniteApiModels.cs):
        /// we use the "all inputs / overall" totals (combined across
        /// solo/duo/squad and every input device) as the single headline
        /// number for this MVP dashboard, rather than offering a per-mode
        /// breakdown. Accuracy is left at 0 ("unknown") because no verified
        /// accuracy field exists in this API's stats response either.
        /// </summary>
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
                Accuracy = 0m, // see assumption note above
                RecordedAt = DateTime.UtcNow
            };
        }
    }
}
