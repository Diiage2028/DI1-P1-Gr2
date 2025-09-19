using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

using static Server.Models.RoundAction;

namespace Server.Actions;

//On est dans un serveur qui gère un jeu à tours (rounds).
public sealed record ActInRoundParams(
    RoundActionType ActionType,
    RoundActionPayload ActionPayload,
    int? RoundId = null,
    Round? Round = null,
    int? PlayerId = null,
    Player? Player = null
);

public class ActInRoundValidator : AbstractValidator<ActInRoundParams>
{
    public ActInRoundValidator()
    {
        RuleFor(p => p.ActionType).NotEmpty();
        RuleFor(p => p.ActionPayload).NotEmpty();
        RuleFor(p => p.RoundId).NotEmpty().When(p => p.Round is null);
        RuleFor(p => p.Round).NotEmpty().When(p => p.RoundId is null);
        RuleFor(p => p.PlayerId).NotEmpty().When(p => p.Player is null);
        RuleFor(p => p.Player).NotEmpty().When(p => p.PlayerId is null);
    }
}

public class ActInRound(
    IRoundsRepository roundsRepository,
    IPlayersRepository playersRepository,
    IAction<FinishRoundParams, Result<Round>> finishRoundAction,
    IGameHubService gameHubService
) : IAction<ActInRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(ActInRoundParams actionParams)
    {
        var actionValidator = new ActInRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (actionType, actionPayload, roundId, round, playerId, player) = actionParams;

        round ??= await roundsRepository.GetById(roundId!.Value);

        if (round is null)
        {
            Result.Fail($"Round with Id \"{roundId}\" not found.");
        }

        player ??= await playersRepository.GetById(playerId!.Value);

        if (player is null)
        {
            Result.Fail($"Player with Id \"{playerId}\" not found.");
        }

        if (!round!.CanPlayerActIn(player!.Id!.Value))
        {
            return Result.Fail("Player cannot act in this round.");
        }

        var roundAction = CreateForType(actionType, player.Id.Value, actionPayload);

        round.Actions.Add(roundAction);

        await roundsRepository.SaveRound(round);

        if (round.EverybodyPlayed())
        {
            var finishRoundParams = new FinishRoundParams(Round: round);
            var finishRoundResult = await finishRoundAction.PerformAsync(finishRoundParams);

            if (finishRoundResult.IsFailed)
            {
                return Result.Fail(finishRoundResult.Errors);
            }
        }
        // IGameHubService → mise à jour temps réel via un hub (ex. SignalR).
        await gameHubService.UpdateCurrentGame(gameId: round.GameId);

        return Result.Ok(round);
    }
}
