using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class StatisticRepository(WssDbContext context) : IStatisticRepository
{
    // Returns the total number of players in the database
    public async Task<int> GetTotalPlayers()
    {
        return await context.Players.CountAsync();
    }

    // Returns the total number of games in the database
    public async Task<int> GetTotalGames()
    {
        return await context.Games.CountAsync();
    }
}
