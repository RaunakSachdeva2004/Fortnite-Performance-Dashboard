using FortniteDashboard.Models;
using FortniteDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortniteDashboard.Controllers
{
    [Authorize]
    public class MatchesController : Controller
    {
        private readonly IMatchService _matchService;
        private readonly IGameModeService _gameModeService;

        public MatchesController(IMatchService matchService, IGameModeService gameModeService)
        {
            _matchService = matchService;
            _gameModeService = gameModeService;
        }

        private int? CurrentPlayerId()
        {
            var claim = User.FindFirst("PlayerId");
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public async Task<IActionResult> Index(string? gameMode, string? searchTerm, string sortBy = "date_desc", int page = 1)
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            var vm = await _matchService.GetPlayerMatchesAsync(playerId.Value, gameMode, searchTerm, sortBy, page, pageSize: 10);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMatch(Match match)
        {
            var playerId = CurrentPlayerId();
            if (playerId is null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid match details provided.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _matchService.AddMatchAsync(playerId.Value, match);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Match recorded successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to add match.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
