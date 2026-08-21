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

        public DashboardController(ApplicationDbContext db, IStatsService statsService)
        {
            _db = db;
            _statsService = statsService;
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
                // Logged in but has no linked Player profile (shouldn't normally happen
                // for the "Player" role given Register() always creates one).
                TempData["Error"] = "No player profile linked to this account.";
                return RedirectToAction("Login", "Account");
            }

            var player = await _db.Players.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player is null)
                return NotFound();

            var stats = await _statsService.GetStatsForPlayerAsync(playerId.Value);
            var recommendations = await _statsService.GetRecommendationsForPlayerAsync(playerId.Value);

            var vm = new DashboardViewModel
            {
                PlayerName = User.FindFirst(ClaimTypes.Name)?.Value ?? player.FortniteUsername,
                FortniteUsername = player.FortniteUsername,
                Team = player.Team,
                Stats = stats,
                Recommendations = recommendations
            };

            if (stats is not null)
            {
                // Single current snapshot for Chart.js. Swap this for a real
                // history table/query if you later track stats over time.
                vm.ChartLabels = new List<string> { "Current" };
                vm.ChartWinRateSeries = new List<decimal> { stats.WinRate };
                vm.ChartKDSeries = new List<decimal> { stats.KDRatio };
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncStats(string username)
        {
            var playerId = CurrentPlayerId();
            if (playerId is null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Enter an Epic username to sync.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _statsService.SyncPlayerStatsAsync(playerId.Value, username.Trim());
                TempData["Success"] = $"Stats synced for {username}.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
