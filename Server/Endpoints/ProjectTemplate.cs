using Microsoft.EntityFrameworkCore;

using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

namespace Server.Endpoints;

public class GetProjectTemplates : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/projects/templates", Handler).WithTags("Projects");
    }

    public static async Task<IResult> Handler(
        IProjectsTemplateRepository projectsTemplateRepository)
    {
        var projects = await projectsTemplateRepository.GetProjectTemplates();
        return Results.Ok(projects);
    }
}
