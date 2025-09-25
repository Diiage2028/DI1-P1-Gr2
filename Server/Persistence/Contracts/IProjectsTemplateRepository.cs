using Server.Models;

namespace Server.Persistence.Contracts;

public interface IProjectsTemplateRepository
{
    Task SaveProject(ProjectTemplate template);
    Task<List<ProjectTemplate>> GetProjectTemplates();
}
