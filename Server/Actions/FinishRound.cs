using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record FinishRoundParams(int? RoundId = null, Round? Round = null);

public class FinishRoundValidator : AbstractValidator<FinishRoundParams>
{
    public FinishRoundValidator()
    {
        RuleFor(p => p.RoundId).NotEmpty().When(p => p.Round is null);
        RuleFor(p => p.Round).NotEmpty().When(p => p.RoundId is null);
    }
}

public class FinishRound(
    IRoundsRepository roundsRepository,
    //IAction<ApplyRoundActionParams, Result> applyRoundActionAction,
    IAction<StartRoundParams, Result<Round>> startRoundAction,
    IAction<FinishGameParams, Result<Game>> finishGameAction,
    IGameHubService gameHubService
) : IAction<FinishRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(FinishRoundParams actionParams)
    {
        var actionValidator = new FinishRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (roundId, round) = actionParams;

        round ??= await roundsRepository.GetById(roundId!.Value);

        if (round is null)
        {
            return Result.Fail($"Round with Id \"{roundId}\" not found.");
        }

        if (round.Game.CanStartANewRound())
        {
            var startRoundActionParams = new StartRoundParams(Game: round.Game);
            var startRoundActionResult = await startRoundAction.PerformAsync(startRoundActionParams);
            var newRound = startRoundActionResult.Value;

            await gameHubService.UpdateCurrentGame(gameId: round.GameId);

            return Result.Ok(newRound);
        }

        return Result.Ok(round);
    }
}
