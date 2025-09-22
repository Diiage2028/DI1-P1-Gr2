using Server.Models;

namespace Server.Persistence.Contracts;

public interface IProjectsRepository
{
    Task SaveProject(Project project);
}
