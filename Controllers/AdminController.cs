using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.Services;
using FortniteDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IGameModeService _gameModeService;
        private readonly IPerformanceService _performanceService;

        public AdminController(ApplicationDbContext db, IGameModeService gameModeService, IPerformanceService performanceService)
        {
            _db = db;
            _gameModeService = gameModeService;
            _performanceService = performanceService;
        }

        public async Task<IActionResult> Index(string? search, string? teamFilter)
        {
            var playersQuery = _db.Players.AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.StatsHistory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                playersQuery = playersQuery.Where(p =>
                    p.FortniteUsername.ToLower().Contains(term) ||
                    (p.User != null && p.User.Name.ToLower().Contains(term)) ||
                    (p.User != null && p.User.Email.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(teamFilter) && !teamFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                playersQuery = playersQuery.Where(p => p.Team != null && p.Team.Equals(teamFilter, StringComparison.OrdinalIgnoreCase));
            }

            var players = await playersQuery.ToListAsync();

            var rows = players.Select(p =>
            {
                var latest = p.StatsHistory.OrderByDescending(s => s.RecordedAt).FirstOrDefault();
                decimal score = latest != null ? (latest.PerformanceScore > 0 ? latest.PerformanceScore : _performanceService.CalculatePerformanceScore(latest)) : 0m;

                return new AdminPlayerRowViewModel
                {
                    PlayerId = p.PlayerId,
                    UserId = p.UserId,
                    UserName = p.User?.Name ?? "(unlinked)",
                    Email = p.User?.Email ?? "-",
                    FortniteUsername = p.FortniteUsername,
                    Team = p.Team,
                    IsActive = p.User?.IsActive ?? true,
                    Eliminations = latest?.Eliminations ?? 0,
                    Wins = latest?.Wins ?? 0,
                    MatchesPlayed = latest?.MatchesPlayed ?? 0,
                    KDRatio = latest?.KDRatio ?? 0,
                    WinRate = latest?.WinRate ?? 0,
                    PerformanceScore = score,
                    LastSyncedAt = latest?.RecordedAt
                };
            }).OrderByDescending(r => r.PerformanceScore).ToList();

            var gameModes = await _gameModeService.GetAllGameModesAsync();
            int totalMatches = await _db.Matches.CountAsync();

            var vm = new AdminDashboardViewModel
            {
                Players = rows,
                GameModes = gameModes,
                TotalPlayers = rows.Count,
                ActivePlayers = rows.Count(r => r.IsActive),
                TotalMatchesPlayed = totalMatches > 0 ? totalMatches : rows.Sum(r => r.MatchesPlayed),
                TotalTeams = rows.Where(r => !string.IsNullOrWhiteSpace(r.Team)).Select(r => r.Team).Distinct().Count(),
                AverageWinRate = rows.Count > 0 ? Math.Round(rows.Average(r => r.WinRate), 2) : 0m,
                AverageKDRatio = rows.Count > 0 ? Math.Round(rows.Average(r => r.KDRatio), 2) : 0m,
                PlatformPerformanceScore = rows.Count > 0 ? Math.Round(rows.Average(r => r.PerformanceScore), 1) : 0m,
                SystemStatus = "Operational"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null) return NotFound();

            if (user.Role == "Administrator")
            {
                TempData["Error"] = "Cannot disable administrator accounts.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"User {user.Name} status updated to {(user.IsActive ? "Active" : "Disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGameMode(GameMode mode)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid game mode data.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _gameModeService.AddGameModeAsync(mode);
            if (result.IsSuccess)
            {
                TempData["Success"] = $"Game Mode '{mode.Name}' added successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to add game mode.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleGameMode(int gameModeId)
        {
            var result = await _gameModeService.ToggleGameModeStatusAsync(gameModeId);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Game mode status toggled.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to update game mode.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
