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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStatsService _statsService;
        private readonly IPerformanceService _performanceService;
        private readonly IGameModeService _gameModeService;

        public ProfileController(
            ApplicationDbContext db,
            IStatsService statsService,
            IPerformanceService performanceService,
            IGameModeService gameModeService)
        {
            _db = db;
            _statsService = statsService;
            _performanceService = performanceService;
            _gameModeService = gameModeService;
        }

        private int? CurrentPlayerId()
        {
            var claim = User.FindFirst("PlayerId");
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public async Task<IActionResult> Index()
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            var player = await _db.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.PlayerId == playerId);
            if (player is null) return NotFound();

            var stats = await _statsService.GetStatsForPlayerAsync(playerId.Value);
            var modes = await _gameModeService.GetActiveGameModesAsync();
            int totalMatches = await _db.Matches.CountAsync(m => m.PlayerId == playerId.Value);

            decimal score = stats != null ? (stats.PerformanceScore > 0 ? stats.PerformanceScore : _performanceService.CalculatePerformanceScore(stats)) : 0m;

            var vm = new ProfileViewModel
            {
                UserId = player.UserId,
                PlayerId = player.PlayerId,
                Name = player.User?.Name ?? string.Empty,
                Email = player.User?.Email ?? string.Empty,
                Role = player.User?.Role ?? "Player",
                FortniteUsername = player.FortniteUsername,
                Team = player.Team,
                Bio = player.Bio,
                PreferredGameMode = player.PreferredGameMode ?? "Solo",
                JoinedDate = player.CreatedDate,
                TotalMatches = totalMatches > 0 ? totalMatches : (stats?.MatchesPlayed ?? 0),
                PerformanceScore = score,
                PerformanceLevel = _performanceService.GetPerformanceLevel(score),
                AvailableGameModes = modes.Select(m => m.Name).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            var player = await _db.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.PlayerId == playerId);
            if (player is null) return NotFound();

            if (player.User != null && !string.IsNullOrWhiteSpace(model.Name))
            {
                player.User.Name = model.Name.Trim();
            }

            player.Team = model.Team?.Trim();
            player.Bio = model.Bio?.Trim();
            player.PreferredGameMode = model.PreferredGameMode;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
