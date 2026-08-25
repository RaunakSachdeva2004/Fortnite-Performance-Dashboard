using FortniteDashboard.Models;
using FortniteDashboard.ViewModels;

namespace FortniteDashboard.Services
{
    public interface IMatchService
    {
        Task<MatchHistoryViewModel> GetPlayerMatchesAsync(int playerId, string? gameMode = null, string? searchTerm = null, string sortBy = "date_desc", int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<Match?> GetMatchDetailsAsync(int matchId, CancellationToken cancellationToken = default);
        Task<Result<Match>> AddMatchAsync(int playerId, Match match, CancellationToken cancellationToken = default);
    }
}
