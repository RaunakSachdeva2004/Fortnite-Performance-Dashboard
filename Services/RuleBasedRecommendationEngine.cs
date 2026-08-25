using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

public class RuleBasedRecommendationEngine : IRecommendationEngine
{
    public List<string> GenerateRecommendations(Stats stats)
    {
        return GenerateStructuredRecommendations(stats)
            .Select(r => r.RecommendationText)
            .ToList();
    }

    public List<Recommendation> GenerateStructuredRecommendations(Stats stats)
    {
        var recommendations = new List<Recommendation>();

        if (stats.MatchesPlayed < 10)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Sample Size",
                Severity = "Low",
                MetricName = "Matches Played",
                RecommendationText = "Play more matches! We need at least 10 matches to provide accurate coaching trends.",
                RecommendedAction = "Complete 10 matches in any game mode to unlock comprehensive performance tracking.",
                CreatedDate = DateTime.UtcNow
            });
            return recommendations;
        }

        // Rule 1: Evaluate K/D Ratio
        if (stats.KDRatio < 1.0m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Combat Efficiency",
                Severity = "High",
                MetricName = "K/D Ratio",
                RecommendationText = "Focus on survival and positioning. A K/D under 1.0 means you are taking disadvantageous fights. Try dropping in quieter POIs.",
                RecommendedAction = "Land at unnamed locations, prioritize shield setup before engaging, and avoid 50/50 early 1v1 fights.",
                CreatedDate = DateTime.UtcNow
            });
        }
        else if (stats.KDRatio >= 3.0m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Combat Efficiency",
                Severity = "Low",
                MetricName = "K/D Ratio",
                RecommendationText = "Excellent K/D ratio! You are consistently out-trading opponents. Work on translating these eliminations into Victory Royales.",
                RecommendedAction = "Use your gun skill to control center storm positions early in third zone.",
                CreatedDate = DateTime.UtcNow
            });
        }

        // Rule 2: Evaluate Win Rate
        if (stats.WinRate < 5.0m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Late Game Conversion",
                Severity = "High",
                MetricName = "Win Rate",
                RecommendationText = "Your win rate is below 5%. Practice late-game rotations and avoid unnecessary fights when there are less than 10 players left.",
                RecommendedAction = "Save materials and mobility items for final moving zones; play edge of storm until Top 5.",
                CreatedDate = DateTime.UtcNow
            });
        }
        else if (stats.WinRate > 15.0m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Late Game Conversion",
                Severity = "Low",
                MetricName = "Win Rate",
                RecommendationText = "Great win rate! Your game sense is strong. Keep leading your team and playing for end-game positioning.",
                RecommendedAction = "Focus on lobby control and securing high ground in fifth zone.",
                CreatedDate = DateTime.UtcNow
            });
        }

        // Rule 3: Evaluate Accuracy
        if (stats.Accuracy > 0 && stats.Accuracy < 15m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Aim & Accuracy",
                Severity = "Medium",
                MetricName = "Accuracy",
                RecommendationText = "Your accuracy is below 15%. Consider lowering your mouse sensitivity or spending 15 minutes a day in aim training maps.",
                RecommendedAction = "Practice tracking and click-timing in Kovaks or Aim Lab for 15 mins daily.",
                CreatedDate = DateTime.UtcNow
            });
        }
        else if (stats.Accuracy > 30m)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Aim & Accuracy",
                Severity = "Low",
                MetricName = "Accuracy",
                RecommendationText = "Incredible aim! With accuracy over 30%, you should play aggressively and look for sniper opportunities.",
                RecommendedAction = "Take mid-to-long range AR tags before initiating team pushes.",
                CreatedDate = DateTime.UtcNow
            });
        }

        // Fallback rule if the player is perfectly average
        if (recommendations.Count == 0)
        {
            recommendations.Add(new Recommendation
            {
                PlayerId = stats.PlayerId,
                Category = "Consistency",
                Severity = "Low",
                MetricName = "Overall",
                RecommendationText = "You are playing very consistently. Keep practicing box fights and building techniques to push your stats to the next tier.",
                RecommendedAction = "Run 15 minutes of edit course warmups before jumping into ranked games.",
                CreatedDate = DateTime.UtcNow
            });
        }

        return recommendations;
    }
}
