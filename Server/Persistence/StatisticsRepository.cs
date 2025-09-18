
using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class StatisticsRepository(WssDbContext context) : IStatisticsRepository
{
    public async Task<int> GetTotalGamesAsync()
        => await context.Games.CountAsync();

    public async Task<int> GetTotalPlayersAsync()
        => await context.Players.CountAsync();
}
