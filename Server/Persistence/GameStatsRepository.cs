using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class GameStatsRepository(WssDbContext context) : IGameStatsRepository
{

    public async Task<GameStat> GetStats()
    {
        try
        {
            var gameCount = await context.Games.CountAsync();
            var playerCount = await context.Players.CountAsync();

            return new GameStat
            {
                GameCount = gameCount,
                PlayerCount = playerCount
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error in GameStatsRepository: {ex.Message}");
            throw new Exception("Failed to retrieve statistics from database", ex);
        }
    }
}
