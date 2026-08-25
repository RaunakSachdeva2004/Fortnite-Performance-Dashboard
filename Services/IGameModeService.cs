using FortniteDashboard.Models;

namespace FortniteDashboard.Services
{
    public interface IGameModeService
    {
        Task<List<GameMode>> GetActiveGameModesAsync(CancellationToken cancellationToken = default);
        Task<List<GameMode>> GetAllGameModesAsync(CancellationToken cancellationToken = default);
        Task<Result<GameMode>> AddGameModeAsync(GameMode gameMode, CancellationToken cancellationToken = default);
        Task<Result<bool>> ToggleGameModeStatusAsync(int gameModeId, CancellationToken cancellationToken = default);
    }
}
