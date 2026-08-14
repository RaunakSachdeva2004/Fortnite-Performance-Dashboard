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

        // Navigation
        public Player? Player { get; set; }
    }

    public class Player
    {
        public int PlayerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FortniteUsername { get; set; } = string.Empty; // Epic username

        [Required, MaxLength(50)]
        public string Game { get; set; } = "Fortnite";

        [MaxLength(100)]
        public string? Team { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public Stats? Stats { get; set; }
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }

    public class Stats
    {
        public int StatId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public int Eliminations { get; set; }
        public int Wins { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Accuracy { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal KDRatio { get; set; }

        public int MatchesPlayed { get; set; }

        // Computed in the database (WinRate AS (...) PERSISTED) — read-only from EF Core
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(6,2)")]
        public decimal WinRate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }

    public class Recommendation
    {
        public int RecommendationId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required, MaxLength(1000)]
        public string RecommendationText { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}
