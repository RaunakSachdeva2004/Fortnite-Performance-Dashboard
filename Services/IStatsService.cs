using FortniteDashboard.Models;

namespace FortniteDashboard.Services
{
    public interface IStatsService
    {
        /// <summary>
        /// Loads the most recent Stats snapshot for a player, if any.
        /// </summary>
        Task<Stats?> GetStatsForPlayerAsync(int playerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads up to <paramref name="take"/> of the player's most recent Stats
        /// snapshots, oldest first (chart-friendly order).
        /// </summary>
        Task<List<Stats>> GetStatsHistoryForPlayerAsync(int playerId, int take = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads the most recent recommendations for a player.
        /// </summary>
        Task<List<Recommendation>> GetRecommendationsForPlayerAsync(int playerId, int take = 5, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calls the Fortnite API for the given player's Epic username, maps the
        /// response into a new Stats snapshot (never overwrites a previous one),
        /// regenerates coaching recommendations from the new stats, persists
        /// everything, and returns the new Stats row. Failures (player not
        /// found, API/network error) come back as a typed failure Result
        /// instead of a thrown exception.
        /// </summary>
        Task<Result<Stats>> SyncPlayerStatsAsync(int playerId, string epicUsername, CancellationToken cancellationToken = default);
    }
}
