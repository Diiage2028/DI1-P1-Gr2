using FluentResults;

using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

namespace Server.Endpoints;

public class GetProjects : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Example: /projects?gameId=1 or /projects?gameId=1&companyId=2
        app.MapGet("projects", Handler).WithTags("Projects");
    }

    public static async Task<IResult> Handler(
        int gameId,
        int? companyId,
        IProjectsRepository projectsRepository
    )
    {
        if (companyId is null)
        {
            var projects = await projectsRepository.GetProjectsGameAvailable(gameId);
            return Results.Ok(projects);
        }
        else
        {
            var projects = await projectsRepository.GetProjectsGameByCompanyId(gameId, companyId.Value);
            return Results.Ok(projects);
        }
    }
}
