using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class ProjectsRepository(WssDbContext context) : IProjectsRepository
{
    public async Task SaveProject(Project project)
    {
        if (project.Id is null)
        {
            await context.AddAsync(project);
        }
        await context.SaveChangesAsync();
    }
}