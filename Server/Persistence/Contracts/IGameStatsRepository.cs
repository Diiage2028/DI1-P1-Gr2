using Server.Models;

namespace Server.Persistence.Contracts;

public interface IGameStatsRepository
{
    Task<GameStat> GetStats();
}
