using System.Security.Claims;
using FortniteDashboard.Data;
using FortniteDashboard.Services;
using FortniteDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStatsService _statsService;
        private readonly IPerformanceService _performanceService;

        public DashboardController(
            ApplicationDbContext db,
            IStatsService statsService,
            IPerformanceService performanceService)
        {
            _db = db;
            _statsService = statsService;
            _performanceService = performanceService;
        }

        private int? CurrentPlayerId()
        {
            var claim = User.FindFirst("PlayerId");
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public async Task<IActionResult> Index()
        {
            var playerId = CurrentPlayerId();
            if (playerId is null)
            {
                TempData["Error"] = "No player profile linked to this account.";
                return RedirectToAction("Login", "Account");
            }

            var player = await _db.Players.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player is null) return NotFound();

            var stats = await _statsService.GetStatsForPlayerAsync(playerId.Value);
            var recommendations = await _statsService.GetRecommendationsForPlayerAsync(playerId.Value);
            var history = await _statsService.GetStatsHistoryForPlayerAsync(playerId.Value, take: 10);
            var recentForm = await _performanceService.GetRecentFormSummaryAsync(playerId.Value);

            var recentMatches = await _db.Matches.AsNoTracking()
                .Where(m => m.PlayerId == playerId.Value)
                .OrderByDescending(m => m.PlayedAt)
                .Take(5)
                .ToListAsync();

            decimal perfScore = stats != null
                ? (stats.PerformanceScore > 0 ? stats.PerformanceScore : _performanceService.CalculatePerformanceScore(stats))
                : 0m;

            var vm = new DashboardViewModel
            {
                PlayerName = User.FindFirst(ClaimTypes.Name)?.Value ?? player.FortniteUsername,
                FortniteUsername = player.FortniteUsername,
                Team = player.Team,
                PreferredGameMode = player.PreferredGameMode ?? "Solo",
                Stats = stats,
                PerformanceScore = perfScore,
                PerformanceLevel = _performanceService.GetPerformanceLevel(perfScore),
                RecentForm = recentForm,
                Recommendations = recommendations,
                History = history,
                RecentMatches = recentMatches,
                ChartLabels = history.Select(s => s.RecordedAt.ToLocalTime().ToString("MMM d, HH:mm")).ToList(),
                ChartWinRateSeries = history.Select(s => s.WinRate).ToList(),
                ChartKDSeries = history.Select(s => s.KDRatio).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncStats(string username)
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Enter an Epic username to sync.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _statsService.SyncPlayerStatsAsync(playerId.Value, username.Trim());

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Stats synced for {username}.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Sync failed. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
