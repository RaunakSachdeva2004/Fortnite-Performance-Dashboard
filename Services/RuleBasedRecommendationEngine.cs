using System.Collections.Generic;
using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

/// <summary>
/// An initial implementation of <see cref="IRecommendationEngine"/> that uses 
/// predetermined if/else logic to evaluate player statistics.
/// </summary>
/// <remarks>
/// This acts as the baseline coaching AI for Phase 1. Because we implement the Strategy Pattern 
/// via <see cref="IRecommendationEngine"/>, this concrete class can easily be substituted by 
/// a trained Machine Learning model or an LLM integration later without affecting the rest of the app.
/// </remarks>
public class RuleBasedRecommendationEngine : IRecommendationEngine
{
    /// <inheritdoc />
    public List<string> GenerateRecommendations(Stats stats)
    {
        var recommendations = new List<string>();

        // We need a minimum sample size to provide meaningful trends
        if (stats.MatchesPlayed < 10)
        {
            recommendations.Add("Play more matches! We need at least 10 matches to provide accurate coaching trends.");
            return recommendations;
        }

        // Rule 1: Evaluate K/D Ratio
        if (stats.KDRatio < 1.0m)
        {
            recommendations.Add("Focus on survival and positioning. A K/D under 1.0 means you are taking disadvantageous fights. Try dropping in quieter POIs.");
        }
        else if (stats.KDRatio >= 3.0m)
        {
            recommendations.Add("Excellent K/D ratio! You are consistently out-trading opponents. Work on translating these eliminations into Victory Royales.");
        }

        // Rule 2: Evaluate Win Rate
        if (stats.WinRate < 5.0m)
        {
            recommendations.Add("Your win rate is below 5%. Practice late-game rotations and avoid unnecessary fights when there are less than 10 players left.");
        }
        else if (stats.WinRate > 15.0m)
        {
            recommendations.Add("Great win rate! Your game sense is strong. Keep leading your team and playing for end-game positioning.");
        }

        // Rule 3: Evaluate Accuracy
        if (stats.Accuracy > 0 && stats.Accuracy < 0.15m)
        {
            recommendations.Add("Your accuracy is below 15%. Consider lowering your mouse sensitivity or spending 15 minutes a day in aim training maps.");
        }
        else if (stats.Accuracy > 0.30m)
        {
            recommendations.Add("Incredible aim! With accuracy over 30%, you should play aggressively and look for sniper opportunities.");
        }

        // Fallback rule if the player is perfectly average across all metrics
        if (recommendations.Count == 0)
        {
            recommendations.Add("You are playing very consistently. Keep practicing box fights and building techniques to push your stats to the next tier.");
        }

        return recommendations;
    }
}
