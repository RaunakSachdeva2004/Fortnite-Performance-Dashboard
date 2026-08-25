using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FortniteDashboard.Tests
{
    public class PerformanceServiceTests
    {
        private static ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CalculatePerformanceScore_ZeroMatches_ReturnsZero()
        {
            using var db = CreateInMemoryDbContext();
            var service = new PerformanceService(db);
            var stats = new Stats { MatchesPlayed = 0 };

            var score = service.CalculatePerformanceScore(stats);

            Assert.Equal(0m, score);
        }

        [Fact]
        public void CalculatePerformanceScore_HighKDAndWinRate_ReturnsHighScore()
        {
            using var db = CreateInMemoryDbContext();
            var service = new PerformanceService(db);
            var stats = new Stats
            {
                MatchesPlayed = 50,
                KDRatio = 4.5m,
                WinRate = 25.0m,
                Accuracy = 35.0m,
                AvgPlacement = 5.0m
            };

            var score = service.CalculatePerformanceScore(stats);
            var level = service.GetPerformanceLevel(score);

            Assert.True(score >= 80m);
            Assert.True(level == "Excellent" || level == "Strong");
        }

        [Fact]
        public async Task GetRecentFormSummaryAsync_CalculatesTrendsCorrectly()
        {
            using var db = CreateInMemoryDbContext();
            var service = new PerformanceService(db);

            db.Matches.AddRange(
                new Match { PlayerId = 1, Eliminations = 5, Deaths = 1, Placement = 1, IsWin = true, PlayedAt = DateTime.UtcNow },
                new Match { PlayerId = 1, Eliminations = 4, Deaths = 1, Placement = 2, IsWin = false, PlayedAt = DateTime.UtcNow.AddHours(-1) },
                new Match { PlayerId = 1, Eliminations = 1, Deaths = 1, Placement = 40, IsWin = false, PlayedAt = DateTime.UtcNow.AddDays(-2) },
                new Match { PlayerId = 1, Eliminations = 0, Deaths = 1, Placement = 50, IsWin = false, PlayedAt = DateTime.UtcNow.AddDays(-3) }
            );
            await db.SaveChangesAsync();

            var summary = await service.GetRecentFormSummaryAsync(1);

            Assert.NotNull(summary);
            Assert.True(summary.RecentKD > 0);
            Assert.Equal(4, summary.MatchesAnalyzed);
        }
    }
}
