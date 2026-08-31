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

            // ---- Stats ----
            // CHANGED: was configured as 1:1 with Players (a unique index on
            // PlayerId), which meant every sync overwrote the same row and no
            // history could ever be kept. This is now a proper 1-to-many
            // history table: many Stats rows per Player, ordered by RecordedAt.
            // The SQL-Server-only computed WinRate column is also gone — WinRate
            // is now just a normal column, computed in C# before SaveChanges.
            modelBuilder.Entity<Stats>(entity =>
            {
                // EF Core's default key convention looks for "Id" or
                // "{ClassName}Id" -- since this class is named "Stats"
                // (plural), that convention would look for "StatsId", not
                // "StatId". Declaring the key explicitly avoids renaming the
                // property everywhere else in the app.
                entity.HasKey(s => s.StatId);

                entity.HasIndex(s => new { s.PlayerId, s.RecordedAt });

                entity.HasOne(s => s.Player)
                      .WithMany(p => p.StatsHistory)
                      .HasForeignKey(s => s.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
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
