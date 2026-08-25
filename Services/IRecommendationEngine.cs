using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

public interface IRecommendationEngine
{
    /// <summary>
    /// Generates legacy string coaching recommendations for backward compatibility.
    /// </summary>
    List<string> GenerateRecommendations(Stats stats);

    /// <summary>
    /// Generates detailed structured coaching recommendations including category, severity, metric name, and actionable drill advice.
    /// </summary>
    List<Recommendation> GenerateStructuredRecommendations(Stats stats);
}
