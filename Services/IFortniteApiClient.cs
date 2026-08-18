using System.Threading.Tasks;
using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

/// <summary>
/// Defines the contract for interacting with the external FortniteAPI.io service.
/// Provides methods to securely fetch real-time game statistics while adhering to API usage rules.
/// </summary>

public interface IFortniteApiClient
{
    /// <summary>
    /// Fetches a player's Fortnite statistics using their Epic Games username.
    /// The process internally handles looking up the Account ID first, and then querying the stats.
    /// Enforces a rate limit of 10 requests per minute as per the free tier, and manages transient errors gracefully.
    /// </summary>
    /// <param name="epicUsername">The player's Epic Games username.</param>
    /// <returns>
    /// A typed <see cref="Result{T}"/> containing the <see cref="FortnitePlayerStatsResponse"/> if successful.
    /// If the player is not found, their profile is set to private, or an API error occurs, 
    /// a failure result is returned with an appropriate error message, avoiding unhandled exceptions.
    /// </returns>
    Task<Result<FortnitePlayerStatsResponse>> GetPlayerStatsAsync(string epicUsername);
}
