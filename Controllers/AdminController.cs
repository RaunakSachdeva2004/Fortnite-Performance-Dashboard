using FortniteDashboard.Data;
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

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var players = await _db.Players
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Stats)
                .OrderByDescending(p => p.Stats != null ? p.Stats.WinRate : 0)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                Players = players.Select(p => new AdminPlayerRowViewModel
                {
                    PlayerId = p.PlayerId,
                    UserName = p.User?.Name ?? "(unlinked)",
                    Email = p.User?.Email ?? "-",
                    FortniteUsername = p.FortniteUsername,
                    Team = p.Team,
                    Eliminations = p.Stats?.Eliminations ?? 0,
                    Wins = p.Stats?.Wins ?? 0,
                    MatchesPlayed = p.Stats?.MatchesPlayed ?? 0,
                    KDRatio = p.Stats?.KDRatio ?? 0,
                    WinRate = p.Stats?.WinRate ?? 0,
                    LastUpdated = p.Stats?.LastUpdated
                }).ToList()
            };

            vm.TotalPlayers = vm.Players.Count;
            vm.TotalTeams = vm.Players
                .Where(p => !string.IsNullOrWhiteSpace(p.Team))
                .Select(p => p.Team)
                .Distinct()
                .Count();
            vm.AverageWinRate = vm.Players.Count > 0
                ? Math.Round(vm.Players.Average(p => p.WinRate), 2)
                : 0;
            vm.AverageKDRatio = vm.Players.Count > 0
                ? Math.Round(vm.Players.Average(p => p.KDRatio), 2)
                : 0;

            return View(vm);
        }
    }
}
