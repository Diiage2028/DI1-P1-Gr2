
using System.Text.Json.Nodes;

using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record ApplyRoundActionParams(
    RoundAction RoundAction,
    int? GameId = null,
    Game? Game = null
);

public class ApplyRoundActionValidator : AbstractValidator<ApplyRoundActionParams>
{
    public ApplyRoundActionValidator()
    {
        RuleFor(p => p.RoundAction).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class ApplyRoundAction(
    IGamesRepository gamesRepository,
    IGameHubService gameHubService
) : IAction<ApplyRoundActionParams, Result>
{
    public async Task<Result> PerformAsync(ApplyRoundActionParams actionParams)
    {
        var actionValidator = new ApplyRoundActionValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (action, gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        // Fix: Use the Type property and enum values instead of class types
        switch (action)
        {
            case SendEmployeeForTraining:
                await HandleSendEmployeeForTraining((SendEmployeeForTrainingRoundAction)roundAction, game);
                break;
            case RoundActionType.ParticipateInProject:
                await HandleParticipateInProject((ParticipateInProjectRoundAction)roundAction, game);
                break;
            case RoundActionType.EnrollInFormation:
                await HandleEnrollInFormation((EnrollInFormationRoundAction)roundAction, game);
                break;
            case RoundActionType.FireAnEmployee:
                await HandleFireAnEmployee((FireAnEmployeeRoundAction)roundAction, game);
                break;
            case RoundActionType.ConfirmRound:
                await HandleConfirmRound((ConfirmRoundAction)roundAction, game);
                break;
            default:
                return Result.Fail($"Unsupported action type: {roundAction.Type}");
        }

        return Result.Ok();
    }
}
