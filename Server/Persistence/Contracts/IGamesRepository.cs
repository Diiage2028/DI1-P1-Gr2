using Server.Models;

namespace Server.Persistence.Contracts;

public interface IGamesRepository
{
    Task<ICollection<Game>> GetJoinable();
    Task<bool> IsGameNameAvailable(string gameName);
    Task<bool> GameExists(int gameId);
    Task<Game?> GetById(int gameId);
    Task<Game?> GetForOverviewById(int gameId);
    Task<Game?> GetByPlayerId(int playerId);
    Task SaveGame(Game game);
    Task<Game?> FinishGame(int gameId);
}
