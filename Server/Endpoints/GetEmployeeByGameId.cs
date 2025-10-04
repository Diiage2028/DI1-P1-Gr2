using FluentResults;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;

namespace Server.Endpoints;

public class GetEmployeeByGameId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/employee/game/{gameId}", Handler).WithTags("Employees");
    }
    public static async Task<IResult> Handler(
       int gameId,
       IAction<GetEmployeesByGameIdParams, Result<List<Employee>>> getEmployeesAction
   )
    {
        var actionParams = new GetEmployeesByGameIdParams(GameId: gameId);

        var actionResult = await getEmployeesAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        return Results.Ok(actionResult.Value);
    }
}
