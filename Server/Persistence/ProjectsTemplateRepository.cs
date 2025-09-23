using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class ProjectsTemplateRepository(WssDbContext context) : IProjectsTemplateRepository
{
    public async Task SaveProject(ProjectTemplate template)
    {
        if (template.Id is null)
        {
            await context.AddAsync(template);
        }
        await context.SaveChangesAsync();
    }
}