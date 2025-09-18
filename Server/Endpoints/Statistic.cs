using Microsoft.AspNetCore.Mvc;

using Server.Endpoints.Contracts;
using Server.Persistence.Contracts;

namespace Server.Endpoints;

public class StatisticEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // group everything under /statistics
        var group = app.MapGroup("statistics").WithTags("Statistics");

        group.MapGet("players/count", GetTotalPlayers);
        group.MapGet("games/count", GetTotalGames);
    }

    private static async Task<IResult> GetTotalPlayers([FromServices] IStatisticRepository statisticRepository)
    {
        var count = await statisticRepository.GetTotalPlayers();
        return Results.Ok(new { TotalPlayers = count });
    }

    private static async Task<IResult> GetTotalGames([FromServices] IStatisticRepository statisticRepository)
    {
        var count = await statisticRepository.GetTotalGames();
        return Results.Ok(new { TotalGames = count });
    }
}
