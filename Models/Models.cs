using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortniteDashboard.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Player"; // "Player" or "Administrator"

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Player? Player { get; set; }
    }

    public class Player
    {
        public int PlayerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FortniteUsername { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Game { get; set; } = "Fortnite";

        [MaxLength(100)]
        public string? Team { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(50)]
        public string? PreferredGameMode { get; set; } = "Solo";

        [MaxLength(256)]
        public string? AvatarUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public ICollection<Stats> StatsHistory { get; set; } = new List<Stats>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }

    public class GameMode
    {
        public int GameModeId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int MaxPlayers { get; set; } = 100;
    }

    public class Match
    {
        public int MatchId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public int? GameModeId { get; set; }

        [Required, MaxLength(50)]
        public string GameModeName { get; set; } = "Solo";

        public int Placement { get; set; }
        public int Eliminations { get; set; }
        public int Deaths { get; set; } = 1;
        public int Assists { get; set; }
        public int DamageDealt { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Accuracy { get; set; }

        public int Score { get; set; }
        public bool IsWin { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        [ForeignKey(nameof(GameModeId))]
        public GameMode? GameMode { get; set; }
    }

    public class Stats
    {
        public int StatId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required, MaxLength(100)]
        public string SyncedUsername { get; set; } = string.Empty;

        public int Eliminations { get; set; }
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public int Deaths { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal KDRatio { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal WinRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Accuracy { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal AvgPlacement { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal AvgDamage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PerformanceScore { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }

    public class Recommendation
    {
        public int RecommendationId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = "General";

        [MaxLength(20)]
        public string Severity { get; set; } = "Medium"; // High, Medium, Low

        [MaxLength(100)]
        public string MetricName { get; set; } = "K/D Ratio";

        [Required, MaxLength(1000)]
        public string RecommendationText { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RecommendedAction { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}
