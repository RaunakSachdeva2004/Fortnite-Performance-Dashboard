namespace FortniteDashboard.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<AdminPlayerRowViewModel> Players { get; set; } = new();
        public int TotalPlayers { get; set; }
        public int TotalTeams { get; set; }
        public decimal AverageWinRate { get; set; }
        public decimal AverageKDRatio { get; set; }
    }

    public class AdminPlayerRowViewModel
    {
        public int PlayerId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FortniteUsername { get; set; } = string.Empty;
        public string? Team { get; set; }
        public int Eliminations { get; set; }
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public decimal KDRatio { get; set; }
        public decimal WinRate { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }
}
