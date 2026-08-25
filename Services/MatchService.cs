using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Services
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _db;

        public MatchService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MatchHistoryViewModel> GetPlayerMatchesAsync(
            int playerId,
            string? gameMode = null,
            string? searchTerm = null,
            string sortBy = "date_desc",
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Matches.AsNoTracking()
                .Include(m => m.GameMode)
                .Where(m => m.PlayerId == playerId);

            if (!string.IsNullOrWhiteSpace(gameMode) && !gameMode.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.GameModeName.Equals(gameMode, StringComparison.OrdinalIgnoreCase) || (m.GameMode != null && m.GameMode.Name.Equals(gameMode, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(m => m.GameModeName.ToLower().Contains(term) || m.Placement.ToString().Contains(term) || m.Eliminations.ToString().Contains(term));
            }

            query = sortBy switch
            {
                "date_asc" => query.OrderBy(m => m.PlayedAt),
                "kills_desc" => query.OrderByDescending(m => m.Eliminations),
                "kills_asc" => query.OrderBy(m => m.Eliminations),
                "placement_asc" => query.OrderBy(m => m.Placement),
                "damage_desc" => query.OrderByDescending(m => m.DamageDealt),
                _ => query.OrderByDescending(m => m.PlayedAt)
            };

            int totalMatches = await query.CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalMatches / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var matches = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var availableModes = await _db.GameModes.AsNoTracking()
                .Where(g => g.IsActive)
                .Select(g => g.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            return new MatchHistoryViewModel
            {
                Matches = matches,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalMatches = totalMatches,
                SelectedGameMode = gameMode,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                AvailableGameModes = availableModes
            };
        }

        public async Task<Match?> GetMatchDetailsAsync(int matchId, CancellationToken cancellationToken = default)
        {
            return await _db.Matches.AsNoTracking()
                .Include(m => m.GameMode)
                .Include(m => m.Player)
                .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken);
        }

        public async Task<Result<Match>> AddMatchAsync(int playerId, Match match, CancellationToken cancellationToken = default)
        {
            var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);
            if (player is null) return Result<Match>.Failure("Player not found.");

            match.PlayerId = playerId;
            match.PlayedAt = match.PlayedAt == default ? DateTime.UtcNow : match.PlayedAt;
            match.IsWin = match.Placement == 1;

            _db.Matches.Add(match);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Match>.Success(match);
        }
    }
}
