using FortniteDashboard.Data;
using FortniteDashboard.Services;
using FortniteDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Controllers
{
    [Authorize]
    public class CoachingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStatsService _statsService;
        private readonly IPerformanceService _performanceService;

        public CoachingController(ApplicationDbContext db, IStatsService statsService, IPerformanceService performanceService)
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
            if (playerId is null) return RedirectToAction("Login", "Account");

            var player = await _db.Players.AsNoTracking().Include(p => p.User).FirstOrDefaultAsync(p => p.PlayerId == playerId);
            var stats = await _statsService.GetStatsForPlayerAsync(playerId.Value);
            var recs = await _statsService.GetRecommendationsForPlayerAsync(playerId.Value, take: 20);

            decimal score = stats != null ? (stats.PerformanceScore > 0 ? stats.PerformanceScore : _performanceService.CalculatePerformanceScore(stats)) : 0m;

            var vm = new CoachingViewModel
            {
                PlayerName = player?.User?.Name ?? "Player",
                FortniteUsername = player?.FortniteUsername ?? "Gamer",
                PerformanceScore = score,
                PerformanceLevel = _performanceService.GetPerformanceLevel(score),
                Recommendations = recs,
                TopFocusAreas = recs.Where(r => r.Severity == "High").Take(3).ToList()
            };

            if (vm.TopFocusAreas.Count == 0 && recs.Count > 0)
            {
                vm.TopFocusAreas = recs.Take(3).ToList();
            }

            return View(vm);
        }
    }
}
