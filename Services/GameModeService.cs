using FortniteDashboard.Data;
using FortniteDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace FortniteDashboard.Services
{
    public class GameModeService : IGameModeService
    {
        private readonly ApplicationDbContext _db;

        public GameModeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<GameMode>> GetActiveGameModesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.GameModes.AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GameMode>> GetAllGameModesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.GameModes.AsNoTracking()
                .OrderBy(g => g.GameModeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<GameMode>> AddGameModeAsync(GameMode gameMode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(gameMode.Name) || string.IsNullOrWhiteSpace(gameMode.Code))
            {
                return Result<GameMode>.Failure("Name and Code are required.");
            }

            if (await _db.GameModes.AnyAsync(g => g.Code.ToUpper() == gameMode.Code.ToUpper(), cancellationToken))
            {
                return Result<GameMode>.Failure("A game mode with this code already exists.");
            }

            gameMode.Code = gameMode.Code.ToUpper().Trim();
            _db.GameModes.Add(gameMode);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<GameMode>.Success(gameMode);
        }

        public async Task<Result<bool>> ToggleGameModeStatusAsync(int gameModeId, CancellationToken cancellationToken = default)
        {
            var mode = await _db.GameModes.FirstOrDefaultAsync(g => g.GameModeId == gameModeId, cancellationToken);
            if (mode is null) return Result<bool>.Failure("Game mode not found.");

            mode.IsActive = !mode.IsActive;
            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(mode.IsActive);
        }
    }
}
