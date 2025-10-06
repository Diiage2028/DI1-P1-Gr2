
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record FinishGameParams(int? GameId = null, Game? Game = null);

public class FinishGameValidator : AbstractValidator<FinishGameParams>
{
    public FinishGameValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class FinishGame(IGamesRepository gamesRepository) : IAction<FinishGameParams, Result<Game>>
{
    public async Task<Result<Game>> PerformAsync(FinishGameParams actionParams)
    {
        var actionValidator = new FinishGameValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (!actionValidationResult.IsValid)
        {
            return Result.Fail<Game>(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        // Get game ID from either parameter
        int gameId;
        if (actionParams.Game != null)
        {
            gameId = actionParams.Game.Id ?? throw new InvalidOperationException("Game must have an ID");
        }
        else
        {
            gameId = actionParams.GameId!.Value;
        }

        // Verify game exists
        var game = await gamesRepository.GetById(gameId);
        if (game is null)
        {
            return Result.Fail<Game>($"Game with id {gameId} not found");
        }

        // Finish the game and return the updated game
        var finishedGame = await gamesRepository.FinishGame(gameId);
        return Result.Ok(finishedGame);
    }
}
