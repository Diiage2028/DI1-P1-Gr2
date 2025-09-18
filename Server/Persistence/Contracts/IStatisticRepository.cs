using Server.Models;

namespace Server.Persistence.Contracts;

public interface IStatisticRepository
{
    //Task<ICollection<Player>> GetAllPlayers();
    Task<int> GetTotalPlayers();
    Task<int> GetTotalGames();
}
