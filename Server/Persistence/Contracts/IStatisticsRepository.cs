using Server.Models;

namespace Server.Persistence.Contracts;

public interface IStatisticsRepository
{
    Task<int> GetTotalGamesAsync();
    Task<int> GetTotalPlayersAsync();
}
