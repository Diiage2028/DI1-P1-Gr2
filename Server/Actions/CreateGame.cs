using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateGameParams(string GameName, string PlayerName, string CompanyName, int Rounds = 5);

public class CreateGameValidator : AbstractValidator<CreateGameParams>
{
    public CreateGameValidator()
    {
        RuleFor(p => p.GameName).NotEmpty();
        RuleFor(p => p.PlayerName).NotEmpty();
        RuleFor(p => p.CompanyName).NotEmpty();
        RuleFor(p => p.Rounds).NotNull().GreaterThanOrEqualTo(5).LessThanOrEqualTo(20);
    }
}

public class CreateGame(
  IGamesRepository gamesRepository,
//   IProjectsRepository projectsRepository,
  IAction<CreatePlayerParams, Result<Player>> createPlayerAction,
  IMainHubService mainHubService
) : IAction<CreateGameParams, Result<Game>>
{
    public async Task<Result<Game>> PerformAsync(CreateGameParams actionParams)
    {
        var actionValidator = new CreateGameValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (gameName, playerName, companyName, rounds) = actionParams;

        var isGameNameAvailable = await gamesRepository.IsGameNameAvailable(gameName);

        if (!isGameNameAvailable)
        {
            return Result.Fail("'Game Name' is already in use.");
        }

        var game = new Game(gameName, rounds);

        await gamesRepository.SaveGame(game);

        var createPlayerParams = new CreatePlayerParams(playerName, companyName, Game: game);
        var createPlayerResult = await createPlayerAction.PerformAsync(createPlayerParams);

        if (createPlayerResult.IsFailed)
        {
            return Result.Fail(createPlayerResult.Errors);
        }


        // // Notify clients that the game state has changed
        // await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);

        await mainHubService.UpdateJoinableGamesList();
        

        return Result.Ok(game);
    }
}
