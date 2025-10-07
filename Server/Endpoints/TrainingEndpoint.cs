using FluentResults;

using Microsoft.AspNetCore.Mvc;

using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;

namespace Server.Endpoints;

public class TrainingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/gettraining", Handler).WithTags("Training");
    }
    public static async Task<IResult> Handler(
        [FromServices] IAction<GetTrainingParams, Result<List<Training>>> getTrainingAction
    )
    {
        var actionParams = new GetTrainingParams();
        var actionResult = await getTrainingAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        return Results.Ok(actionResult.Value);
    }
}
