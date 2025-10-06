using Microsoft.EntityFrameworkCore;

using Server.Actions;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class GamesRepository(WssDbContext context) : IGamesRepository
{
    public async Task<ICollection<Game>> GetJoinable()
    {
        return await context.Games
            .Include(g => g.Players)
            .Where(g => g.Status == GameStatus.Waiting)
            .Where(g => g.Players.Count < 3)
            .ToListAsync();
    }

    public async Task<bool> GameExists(int gameId)
    {
        return await context.Games.AnyAsync(game => game.Id == gameId);
    }

    public async Task<bool> IsGameNameAvailable(string gameName)
    {
        return !await context.Games.AnyAsync(game => game.Name == gameName);
    }

    public async Task<Game?> GetById(int gameId)
    {
        return await context.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game?> GetForOverviewById(int gameId)
    {
        return await context.Games
            .Include(g => g.Players).ThenInclude(p => p.Company).ThenInclude(c => c.Employees).ThenInclude(e => e.Skills)
            .Include(g => g.Consultants).ThenInclude(c => c.Skills)
            .Include(g => g.RoundsCollection)
            .Include(g => g.RoundsCollection).ThenInclude(a => a.Actions)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game?> GetByPlayerId(int playerId)
    {
        return await context.Games
            .Join(
                context.Players,
                game => game.Id,
                player => player.GameId,
                (game, player) => new { Game = game, Player = player }
            )
            .Where(res => res.Player.Id == playerId)
            .Select(res => res.Game)
            .FirstOrDefaultAsync();
    }

    public async Task SaveGame(Game game)
    {
        if (game.Id is null)
        {
            await context.AddAsync(game);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Game> FinishGame(int gameId)
    {
        var game = await context.Games.FindAsync(gameId);
        if (game != null)
        {
            game.Status = GameStatus.Finished;
            await context.SaveChangesAsync();
        }
        return game; // Return the updated game
    }
}
