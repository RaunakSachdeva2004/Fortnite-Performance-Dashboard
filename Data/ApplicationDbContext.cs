using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FortniteDashboard.Models;

namespace FortniteDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Stats> Stats => Set<Stats>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<GameMode> GameModes => Set<GameMode>();
        public DbSet<Recommendation> Recommendations => Set<Recommendation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Users ----
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
            });

            // ---- Players ----
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.UserId).IsUnique();
                entity.HasIndex(p => p.FortniteUsername).IsUnique();

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Player)
                      .HasForeignKey<Player>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Game Modes ----
            modelBuilder.Entity<GameMode>(entity =>
            {
                entity.HasKey(g => g.GameModeId);
                entity.HasIndex(g => g.Code).IsUnique();
            });

            // ---- Matches ----
            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(m => m.MatchId);
                entity.HasIndex(m => new { m.PlayerId, m.PlayedAt });

                entity.HasOne(m => m.Player)
                      .WithMany(p => p.Matches)
                      .HasForeignKey(m => m.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.GameMode)
                      .WithMany()
                      .HasForeignKey(m => m.GameModeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ---- Stats History ----
            modelBuilder.Entity<Stats>(entity =>
            {
                entity.HasKey(s => s.StatId);
                entity.HasIndex(s => new { s.PlayerId, s.RecordedAt });

                entity.HasOne(s => s.Player)
                      .WithMany(p => p.StatsHistory)
                      .HasForeignKey(s => s.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Recommendations ----
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasKey(r => r.RecommendationId);
                entity.HasIndex(r => new { r.PlayerId, r.CreatedDate });

                entity.HasOne(r => r.Player)
                      .WithMany(p => p.Recommendations)
                      .HasForeignKey(r => r.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Seed Initial Data ----
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Game Modes
            modelBuilder.Entity<GameMode>().HasData(
                new GameMode { GameModeId = 1, Name = "Battle Royale Solo", Code = "SOLO", Description = "100 players individual battle royale mode", IsActive = true, MaxPlayers = 100 },
                new GameMode { GameModeId = 2, Name = "Battle Royale Duos", Code = "DUOS", Description = "50 teams of 2 players", IsActive = true, MaxPlayers = 100 },
                new GameMode { GameModeId = 3, Name = "Battle Royale Squads", Code = "SQUADS", Description = "25 teams of 4 players", IsActive = true, MaxPlayers = 100 },
                new GameMode { GameModeId = 4, Name = "Zero Build Solo", Code = "ZB_SOLO", Description = "Tactical combat without building mechanics", IsActive = true, MaxPlayers = 100 },
                new GameMode { GameModeId = 5, Name = "Ranked Battle Royale", Code = "RANKED_BR", Description = "Competitive skill-based ranked queue", IsActive = true, MaxPlayers = 100 }
            );

            // Seed Admin & Demo Users
            var hasher = new PasswordHasher<User>();

            var adminUser = new User
            {
                UserId = 1,
                Name = "System Administrator",
                Email = "admin@fortnitedashboard.local",
                Role = "Administrator",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "ChangeMe123!");

            var demoUser1 = new User
            {
                UserId = 2,
                Name = "Ninja_Pro",
                Email = "ninja@esports.local",
                Role = "Player",
                CreatedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            };
            demoUser1.PasswordHash = hasher.HashPassword(demoUser1, "Player123!");

            var demoUser2 = new User
            {
                UserId = 3,
                Name = "Bucey_FTW",
                Email = "bucey@esports.local",
                Role = "Player",
                CreatedDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            };
            demoUser2.PasswordHash = hasher.HashPassword(demoUser2, "Player123!");

            modelBuilder.Entity<User>().HasData(adminUser, demoUser1, demoUser2);

            // Seed Demo Players
            var player1 = new Player
            {
                PlayerId = 1,
                UserId = 2,
                FortniteUsername = "Ninja_Pro",
                Game = "Fortnite",
                Team = "FaZe Clan",
                Bio = "Competitive Fortnite player focusing on late-game rotations and tournament play.",
                PreferredGameMode = "Ranked Battle Royale",
                CreatedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            var player2 = new Player
            {
                PlayerId = 2,
                UserId = 3,
                FortniteUsername = "Bucey_FTW",
                Game = "Fortnite",
                Team = "Team Liquid",
                Bio = "Zero Build specialist and aim practitioner.",
                PreferredGameMode = "Zero Build Solo",
                CreatedDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<Player>().HasData(player1, player2);

            // Seed Stats History
            modelBuilder.Entity<Stats>().HasData(
                new Stats
                {
                    StatId = 1,
                    PlayerId = 1,
                    SyncedUsername = "Ninja_Pro",
                    Eliminations = 145,
                    Wins = 12,
                    MatchesPlayed = 50,
                    Deaths = 38,
                    KDRatio = 3.82m,
                    WinRate = 24.0m,
                    Accuracy = 28.5m,
                    AvgPlacement = 8.2m,
                    AvgDamage = 450.0m,
                    PerformanceScore = 84.5m,
                    RecordedAt = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)
                },
                new Stats
                {
                    StatId = 2,
                    PlayerId = 1,
                    SyncedUsername = "Ninja_Pro",
                    Eliminations = 188,
                    Wins = 18,
                    MatchesPlayed = 65,
                    Deaths = 47,
                    KDRatio = 4.00m,
                    WinRate = 27.69m,
                    Accuracy = 31.2m,
                    AvgPlacement = 6.4m,
                    AvgDamage = 520.0m,
                    PerformanceScore = 89.0m,
                    RecordedAt = new DateTime(2026, 2, 20, 15, 30, 0, DateTimeKind.Utc)
                },
                new Stats
                {
                    StatId = 3,
                    PlayerId = 2,
                    SyncedUsername = "Bucey_FTW",
                    Eliminations = 42,
                    Wins = 2,
                    MatchesPlayed = 30,
                    Deaths = 28,
                    KDRatio = 1.50m,
                    WinRate = 6.67m,
                    Accuracy = 19.8m,
                    AvgPlacement = 18.5m,
                    AvgDamage = 280.0m,
                    PerformanceScore = 61.2m,
                    RecordedAt = new DateTime(2026, 2, 18, 10, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Initial Matches
            modelBuilder.Entity<Match>().HasData(
                new Match
                {
                    MatchId = 1,
                    PlayerId = 1,
                    GameModeId = 5,
                    GameModeName = "Ranked Battle Royale",
                    Placement = 1,
                    Eliminations = 8,
                    Deaths = 0,
                    Assists = 3,
                    DamageDealt = 1420,
                    Accuracy = 34.5m,
                    Score = 2100,
                    IsWin = true,
                    PlayedAt = new DateTime(2026, 2, 20, 14, 0, 0, DateTimeKind.Utc)
                },
                new Match
                {
                    MatchId = 2,
                    PlayerId = 1,
                    GameModeId = 1,
                    GameModeName = "Battle Royale Solo",
                    Placement = 3,
                    Eliminations = 5,
                    Deaths = 1,
                    Assists = 0,
                    DamageDealt = 980,
                    Accuracy = 29.0m,
                    Score = 1450,
                    IsWin = false,
                    PlayedAt = new DateTime(2026, 2, 20, 15, 0, 0, DateTimeKind.Utc)
                },
                new Match
                {
                    MatchId = 3,
                    PlayerId = 2,
                    GameModeId = 4,
                    GameModeName = "Zero Build Solo",
                    Placement = 12,
                    Eliminations = 2,
                    Deaths = 1,
                    Assists = 1,
                    DamageDealt = 410,
                    Accuracy = 21.0m,
                    Score = 680,
                    IsWin = false,
                    PlayedAt = new DateTime(2026, 2, 18, 9, 30, 0, DateTimeKind.Utc)
                }
            );

            // Seed Coaching Recommendations
            modelBuilder.Entity<Recommendation>().HasData(
                new Recommendation
                {
                    RecommendationId = 1,
                    PlayerId = 1,
                    Category = "Combat Efficiency",
                    Severity = "Low",
                    MetricName = "K/D Ratio",
                    RecommendationText = "Excellent K/D ratio (4.00)! You are consistently winning trades.",
                    RecommendedAction = "Keep maintaining height advantage during late-game box fights.",
                    CreatedDate = new DateTime(2026, 2, 20, 15, 31, 0, DateTimeKind.Utc)
                },
                new Recommendation
                {
                    RecommendationId = 2,
                    PlayerId = 2,
                    Category = "Late Game Conversion",
                    Severity = "High",
                    MetricName = "Win Rate",
                    RecommendationText = "Your win rate is 6.67%. You are reaching mid-game but struggling in final circles.",
                    RecommendedAction = "Practice early rotations to secure high ground in moving storm zones.",
                    CreatedDate = new DateTime(2026, 2, 18, 10, 1, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
