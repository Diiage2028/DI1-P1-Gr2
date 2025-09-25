using Microsoft.EntityFrameworkCore;

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
    public async Task<List<Project>> GetProjectsGameAvailable(int gameId)
    {
        return await context.Projects
            .Where(p => p.GameId == gameId && p.CompanyId == null)
            .ToListAsync();
    }
    public async Task<List<Project>> GetProjectsGameByCompanyId(int gameId, int companyId)
    {
        return await context.Projects
            .Where(p => p.GameId == gameId && p.CompanyId == companyId)
            .ToListAsync();
    }
}
