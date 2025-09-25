using Microsoft.EntityFrameworkCore;

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

    public async Task<List<ProjectTemplate>> GetProjectTemplates()
    {
        return await context.ProjectTemplates.ToListAsync();
    }
}
