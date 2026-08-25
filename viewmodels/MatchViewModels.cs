using FortniteDashboard.Models;

namespace FortniteDashboard.ViewModels
{
    public class MatchHistoryViewModel
    {
        public List<Match> Matches { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalMatches { get; set; }
        public string? SelectedGameMode { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "date_desc";
        public List<string> AvailableGameModes { get; set; } = new();
    }

    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Player";

        public string FortniteUsername { get; set; } = string.Empty;
        public string? Team { get; set; }
        public string? Bio { get; set; }
        public string PreferredGameMode { get; set; } = "Solo";
        public DateTime JoinedDate { get; set; }

        public int TotalMatches { get; set; }
        public decimal PerformanceScore { get; set; }
        public string PerformanceLevel { get; set; } = "Developing";

        public List<string> AvailableGameModes { get; set; } = new();
    }

    public class CoachingViewModel
    {
        public string PlayerName { get; set; } = string.Empty;
        public string FortniteUsername { get; set; } = string.Empty;
        public decimal PerformanceScore { get; set; }
        public string PerformanceLevel { get; set; } = "Developing";

        public List<Recommendation> Recommendations { get; set; } = new();
        public List<Recommendation> TopFocusAreas { get; set; } = new();
    }
}
