using System.Collections.Generic;
using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

/// <summary>
/// Defines the strategy for generating coaching recommendations based on a player's statistics.
/// </summary>
/// <remarks>
/// By abstracting the recommendation logic behind this interface, we are applying the Strategy Pattern. 
/// This decouples the application from any specific implementation. Currently, we use a simple 
/// rule-based engine. In the future, this architectural design allows us to seamlessly swap in a 
/// trained Machine Learning model or an external LLM (Large Language Model) service via dependency 
/// injection without changing any code in the controllers or domain layer.
/// </remarks>
public interface IRecommendationEngine
{
    /// <summary>
    /// Analyzes the provided player statistics and generates actionable coaching tips.
    /// </summary>
    /// <param name="stats">The player's performance statistics (e.g., Eliminations, Wins, Accuracy, KDRatio).</param>
    /// <returns>A list of coaching recommendations formatted as text strings.</returns>
    List<string> GenerateRecommendations(Stats stats);
}
