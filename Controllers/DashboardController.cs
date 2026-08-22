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

            // Ownership note: playerId comes only from the signed-in user's own
            // claim, never from a route/query parameter, so there is no way for
            // a player to request another player's dashboard by editing the URL.
            var player = await _db.Players.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);

            if (player is null)
                return NotFound();

            var stats = await _statsService.GetStatsForPlayerAsync(playerId.Value);
            var recommendations = await _statsService.GetRecommendationsForPlayerAsync(playerId.Value);

            // CHANGED: previously a single hardcoded "Current" point, because
            // Stats was 1:1 with Player and no history existed. Now that Stats
            // is a proper history table, pull the real last-N snapshots so the
            // trend charts show actual progress across syncs.
            var history = await _statsService.GetStatsHistoryForPlayerAsync(playerId.Value, take: 10);

            var vm = new DashboardViewModel
            {
                PlayerName = User.FindFirst(ClaimTypes.Name)?.Value ?? player.FortniteUsername,
                FortniteUsername = player.FortniteUsername,
                Team = player.Team,
                Stats = stats,
                Recommendations = recommendations,
                History = history,
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
            if (playerId is null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Enter an Epic username to sync.";
                return RedirectToAction(nameof(Index));
            }

            // CHANGED: SyncPlayerStatsAsync now returns a typed Result<Stats>
            // instead of throwing InvalidOperationException for expected
            // failure cases (player not found, API/network error, rate limit).
            // That keeps unexpected exceptions from ever reaching the user as a
            // raw error page, per the "typed result" requirement.
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
