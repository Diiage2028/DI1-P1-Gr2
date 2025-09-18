using FluentResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;

namespace Server.Endpoints;

public class Statistics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // GET /statistics/global
        app.MapGet("/statistics/global", Handler)
           .WithTags("Statistics");
    }

    public static async Task<IResult> Handler(
        [FromServices]
        IAction<GetStatisticsParams, Result<StatisticsResult>> getStatisticsAction
    )
    {
        var actionResult = await getStatisticsAction.PerformAsync(new GetStatisticsParams());

        if (actionResult.IsFailed)
        {
            return Results.Problem("Impossible de récupérer les statistiques");
        }

        return Results.Ok(actionResult.Value);
    }
}

