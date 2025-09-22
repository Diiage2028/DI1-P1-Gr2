using System.Text.Json;

using FluentResults;

using Microsoft.AspNetCore.Mvc;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;

using static Server.Models.FireAnEmployeeRoundAction;
using static Server.Models.ParticipateInCallForTendersRoundAction;
using static Server.Models.RecruitAConsultantRoundAction;
using static Server.Models.RoundAction;
using static Server.Models.SendEmployeeForTrainingRoundAction;

namespace Server.Endpoints;

public class ActInRound : IEndpoint
{
    public sealed record ActInRoundBody(string ActionType, string ActionPayload, int PlayerId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("rounds/{roundId}/act", Handler).WithTags("Rounds");
    }

    public static async Task<IResult> Handler(
        int roundId,
        [FromBody] ActInRoundBody body,
        WssDbContext context,
        IAction<ActInRoundParams, Result<Round>> actInRoundAction
    )
    {
        var actionTypeParsed = Enum.TryParse<RoundActionType>(body.ActionType, out var parsedActionType);

        if (!actionTypeParsed)
        {
            return Results.BadRequest(new { Errors = new[] { "Invalid action type" } });
        }

        var parsedActionPayload = parsedActionType switch
        {
            RoundActionType.SendEmployeeForTraining => JsonSerializer.Deserialize<SendEmployeeForTrainingPayload>(body.ActionPayload),
            RoundActionType.ParticipateInCallForTenders => JsonSerializer.Deserialize<ParticipateInCallForTendersPayload>(body.ActionPayload),
            RoundActionType.RecruitAConsultant => JsonSerializer.Deserialize<RecruitAConsultantPayload>(body.ActionPayload),
            RoundActionType.FireAnEmployee => JsonSerializer.Deserialize<FireAnEmployeePayload>(body.ActionPayload),
            RoundActionType.ConfirmRound => JsonSerializer.Deserialize<RoundActionPayload>(body.ActionPayload),
            _ => JsonSerializer.Deserialize<RoundActionPayload>(body.ActionPayload)
        };

        var actionParams = new ActInRoundParams(
            parsedActionType,
            parsedActionPayload!,
            roundId,
            PlayerId: body.PlayerId
        );

        using var transaction = context.Database.BeginTransaction();

        var actionResult = await actInRoundAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            await transaction.RollbackAsync();
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}
