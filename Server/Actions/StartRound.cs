

using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record StartRoundParams(int? GameId = null, Game? Game = null);

public class StartRoundValidator : AbstractValidator<StartRoundParams>
{
    public StartRoundValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class StartRound(
    IGamesRepository gamesRepository,
    IRoundsRepository roundsRepository,
    IGameHubService gameHubService,
    IProjectsRepository projectsRepository,
    ILogger<StartRound> logger
) : IAction<StartRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(StartRoundParams actionParams)
    {
        Console.WriteLine(">>> StartRound.PerformAsync called <<<");

        var actionValidator = new StartRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        if (!game!.CanStartANewRound())
        {
            return Result.Fail("Game cannot start a new round.");
        }

        
        var rnd = new Random();
        var rdTemplateId = rnd.Next(1, 11);
        logger.LogInformation($"Creating project for game {game.Id} with template {rdTemplateId}");

        try
        {
            var project = new Project(gameId: game.Id!.Value, templateId: rdTemplateId);
            await projectsRepository.SaveProject(project);
            logger.LogInformation("Save project ok");

            game.Projects.Add(project);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Error saving project : {ex}");
        }

        var round = new Round(game.Id!.Value, game.RoundsCollection.Count + 1);

        await roundsRepository.SaveRound(round);

        await gameHubService.UpdateCurrentGame(gameId: round.GameId);

        return Result.Ok(round);
    }
}
