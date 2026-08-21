using FortniteDashboard.Models;

namespace FortniteDashboard.Services
{
    public interface IStatsService
    {
        /// <summary>
        /// Loads the current Stats row for a player from the database, if any.
        /// </summary>
        Task<Stats?> GetStatsForPlayerAsync(int playerId);

        /// <summary>
        /// Loads the most recent recommendations for a player.
        /// </summary>
        Task<List<Recommendation>> GetRecommendationsForPlayerAsync(int playerId, int take = 5);

        /// <summary>
        /// Calls the Fortnite API for the given player's Epic username, maps the
        /// response onto the Stats entity (insert or update), regenerates
        /// coaching recommendations from the new stats, persists everything,
        /// and returns the refreshed Stats row.
        /// </summary>
        Task<Stats> SyncPlayerStatsAsync(int playerId, string epicUsername);
    }
}
