using FluentResults;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;

namespace Server.Endpoints;

public class StatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stats", Handler).WithTags("Stats");
    }

    public static async Task<IResult> Handler(
        IAction<GetStatsParams, Result<GameStat>> getStatsAction
    )
    {
        var actionParams = new GetStatsParams();
        var actionResult = await getStatsAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        return Results.Ok(actionResult.Value);
    }
}
