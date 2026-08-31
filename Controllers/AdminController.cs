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
            // CHANGED: Stats used to be a single 1:1 row per Player (p.Stats),
            // so this used to Include(p => p.Stats) directly. Now that Stats is
            // a history table (StatsHistory), each player is projected against
            // only their most recent snapshot, ordered by RecordedAt.
            var players = await _db.Players
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.StatsHistory)
                .ToListAsync();

            var rows = players
                .Select(p =>
                {
                    var latest = p.StatsHistory.OrderByDescending(s => s.RecordedAt).FirstOrDefault();
                    return new AdminPlayerRowViewModel
                    {
                        PlayerId = p.PlayerId,
                        UserName = p.User?.Name ?? "(unlinked)",
                        Email = p.User?.Email ?? "-",
                        FortniteUsername = p.FortniteUsername,
                        Team = p.Team,
                        Eliminations = latest?.Eliminations ?? 0,
                        Wins = latest?.Wins ?? 0,
                        MatchesPlayed = latest?.MatchesPlayed ?? 0,
                        KDRatio = latest?.KDRatio ?? 0,
                        WinRate = latest?.WinRate ?? 0,
                        LastSyncedAt = latest?.RecordedAt
                    };
                })
                .OrderByDescending(r => r.WinRate)
                .ToList();

            var vm = new AdminDashboardViewModel { Players = rows };

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
