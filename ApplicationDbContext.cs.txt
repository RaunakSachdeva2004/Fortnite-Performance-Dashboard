using Microsoft.EntityFrameworkCore;
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
        public DbSet<Recommendation> Recommendations => Set<Recommendation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---- Users ----
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
            });

            // ---- Players (1:1 with Users) ----
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(p => p.UserId).IsUnique();
                entity.HasIndex(p => p.FortniteUsername).IsUnique();

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Player)
                      .HasForeignKey<Player>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Stats (1:1 with Players) ----
            modelBuilder.Entity<Stats>(entity =>
            {
                entity.HasIndex(s => s.PlayerId).IsUnique();

                entity.HasOne(s => s.Player)
                      .WithOne(p => p.Stats)
                      .HasForeignKey<Stats>(s => s.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Matches the SQL Server PERSISTED computed column
                entity.Property(s => s.WinRate)
                      .HasComputedColumnSql(
                          "CASE WHEN [MatchesPlayed] > 0 THEN CAST([Wins] AS DECIMAL(6,2)) / [MatchesPlayed] * 100 ELSE 0 END",
                          stored: true);
            });

            // ---- Recommendations (many:1 with Players) ----
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasIndex(r => new { r.PlayerId, r.CreatedDate });

                entity.HasOne(r => r.Player)
                      .WithMany(p => p.Recommendations)
                      .HasForeignKey(r => r.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
