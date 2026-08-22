using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

/// <summary>
/// Abstracts all communication with the external Fortnite stats provider
/// (currently Fortnite-API.com). Controllers and Razor views must never call
/// the external API directly -- everything goes through this interface, so
/// the concrete provider can be swapped without touching StatsService,
/// controllers, or views.
/// </summary>
public interface IFortniteApiClient
{
    /// <summary>
    /// Looks up a player's lifetime Battle Royale stats by their Epic
    /// username. Returns a failure Result (never throws) for expected
    /// problems: player not found, private stats, invalid API key, rate
    /// limiting, or network/API downtime.
    /// </summary>
    Task<Result<FortniteOverallStats>> GetPlayerStatsAsync(string epicUsername, CancellationToken cancellationToken = default);
}
