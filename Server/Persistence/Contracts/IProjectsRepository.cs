using Server.Models;

namespace Server.Persistence.Contracts;

public interface IProjectsRepository
{
    Task SaveProject(Project project);

    Task<List<Project>> GetProjectsGameAvailable(int gameId);
    Task<List<Project>> GetProjectsGameByCompanyId(int gameId, int companyId);
}
