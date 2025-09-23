using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateProjectsParams(int TemplateId, int? GameId = null, Game? Game = null);

// Validator that defines rules for CreateEmployeeParams using FluentValidation.
public class CreateProjectValidator : AbstractValidator<CreateProjectsParams>
{
    public CreateProjectValidator()
    {
        // Project name must not be empty
        RuleFor(p => p.TemplateId).NotEmpty();

        // If no Game object is provided, GameId must be provided
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);

        // If no GameId is provided, Game object must be provided
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

// Action of creating a project with dependency injection
public class CreateProject(
    IProjectsRepository projectsRepository,   // Access project data
    // ISkillsRepository skillsRepository,       // Retrieve skills
    IGamesRepository gamesRepository,
    IGameHubService gameHubService            // Notify clients via SignalR
) : IAction<CreateProjectsParams, Result<Project>>
{
    public async Task<Result<Project>> PerformAsync(CreateProjectsParams actionParams)
    {
        var rnd = new Random();

        var actionValidator = new CreateProjectValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        // If validation fails, return failure result with error messages
        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        // Deconstructs the params record into separate variables for easier use.
        var (projectName, gameId, game) = actionParams;

        // if game is null, get the name by its id
        game ??= await gamesRepository.GetById((int) gameId!);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        var randomTemplateId = rnd.Next(1, 11);

        // Create new project
        var project = new Project((int) game!.Id!, randomTemplateId);

        // Save project in repository
        await projectsRepository.SaveProject(project);

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        // Return success with the created project
        return Result.Ok(project);
    }
}
