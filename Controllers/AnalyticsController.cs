using FortniteDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortniteDashboard.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IPerformanceService _performanceService;

        public AnalyticsController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        private int? CurrentPlayerId()
        {
            var claim = User.FindFirst("PlayerId");
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public async Task<IActionResult> Index(string period = "30d")
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            var vm = await _performanceService.GetAnalyticsOverviewAsync(playerId.Value, period);
            return View(vm);
        }
    }
}
