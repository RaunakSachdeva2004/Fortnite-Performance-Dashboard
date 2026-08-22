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

        // CHANGED: was a single 1:1 "Stats" record that got overwritten on every
        // sync, which made historical trend charts impossible. This is now a
        // history table — one row per sync — so charts can show real progress
        // over time. Use IStatsService.GetStatsForPlayerAsync (latest snapshot)
        // or GetStatsHistoryForPlayerAsync (last N snapshots) instead of
        // reaching into this collection directly from controllers/views.
        public ICollection<Stats> StatsHistory { get; set; } = new List<Stats>();

        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }

    /// <summary>
    /// A single performance snapshot for a player, recorded each time they sync.
    /// Multiple rows per player are expected and required for trend charts.
    /// </summary>
    public class Stats
    {
        public int StatId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        // Records exactly which Epic username produced this snapshot. A Player
        // is meant to link one Fortnite account, but nothing stops someone
        // from syncing a different username later (e.g. while testing) --
        // this makes each history row self-documenting instead of silently
        // mixing different accounts' stats into one trend line.
        [Required, MaxLength(100)]
        public string SyncedUsername { get; set; } = string.Empty;


        public int Eliminations { get; set; }
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }

        // CHANGED: Deaths didn't previously exist. It's derived (not supplied by
        // FortniteAPI.io — see StatsCalculator) as MatchesPlayed - Wins, since a
        // non-winning match counts as one death for K/D purposes. This lets
        // KDRatio be computed the way the spec asks for (eliminations / deaths)
        // instead of trusting a pre-computed ratio from the API.
        public int Deaths { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal KDRatio { get; set; }

        // CHANGED: previously a SQL Server "PERSISTED computed column" — that
        // T-SQL syntax has no SQLite equivalent EF Core can migrate cleanly.
        // WinRate is now a normal stored column, computed once in C#
        // (StatsCalculator.ComputeWinRate) at sync time. Works identically on
        // every database EF Core supports, not just SQL Server.
        [Column(TypeName = "decimal(6,2)")]
        public decimal WinRate { get; set; }

        // NOTE (isolated assumption): FortniteAPI.io's public /v1/stats response
        // does not expose a weapon-accuracy percentage in any field we could
        // verify. Rather than invent one, Accuracy defaults to 0 ("unknown") and
        // is only used by the recommendation engine when > 0. If you find the
        // real field (or add a manual input for it), wire it up in
        // StatsService.MapApiResponseToSnapshot — that's the one place this
        // assumption lives.
        [Column(TypeName = "decimal(5,2)")]
        public decimal Accuracy { get; set; } // percentage, e.g. 23.45 means 23.45%

        // RENAMED from LastUpdated: this row is now an immutable historical
        // snapshot rather than a row that gets "updated", so RecordedAt (matching
        // the brief's suggested schema) better describes what it is.
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

        [Required, MaxLength(1000)]
        public string RecommendationText { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}
