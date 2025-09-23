using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateProjectsParams(string ProjectName, int? GameId = null, Game? Game = null);

// Validator that defines rules for CreateEmployeeParams using FluentValidation.
public class CreateProjectValidator : AbstractValidator<CreateProjectsParams>
{
    public CreateProjectValidator()
    {
        // Project name must not be empty
        RuleFor(p => p.ProjectName).NotEmpty();

        // If no Game object is provided, GameId must be provided
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);

        // If no GameId is provided, Game object must be provided
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

// Action of creating a project with dependency injection
public class CreateProject(
    IProjectsRepository projectsRepository,   // Access project data
    ISkillsRepository skillsRepository,       // Retrieve skills
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

        // List of names that will be picked randomly for the project
        IEnumerable<string> name =
        [
            "Web app",
            "Mini game job hunting turn by turn",
            "E-commerce platform",
            "Mobile banking app",
            "Inventory management system",
            "Social networking site",
            "Online booking system",
            "Chatbot assistant",
            "Learning management system",
            "IoT smart home dashboard",
            "Data visualization tool",
            "Cloud file storage service"
        ];
        var index = rnd.Next(name.Count()); // random number between 0 and Count-1
        string randomName = name.ElementAt(index);

        var randomRounds = rnd.Next(1, 7);

        // Create the amount for the reward of the projects from a list
        IEnumerable<int> reward = [];

        for (var amount = 90000; amount <= 25000; amount += 500)
        {
            reward = reward.Append(amount);
        }
        var randomReward = reward.ToList()[rnd.Next(reward.Count() - 1)];

        // Create new project
        var project = new Project(randomName, (int) game!.Id!, randomRounds, randomReward);

        // Fetch 3 random skills from repository
        var randomSkills = await skillsRepository.GetRandomSkills(3);

        // Assign each skill to the project with a random level (0–10)
        foreach (var randomSkill in randomSkills)
        {
            project.Skills.Add(new LeveledSkill(randomSkill.Name, rnd.Next(11)));
        }

        // Save project in repository
        await projectsRepository.SaveProject(project);

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        // Return success with the created project
        return Result.Ok(project);
    }
}
