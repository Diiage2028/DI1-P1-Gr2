using Server.Hubs.Records;

namespace Server.Models;

public class Project(int gameId, int templateId)
{
    public int? Id { get; private set; }
    public int GameId { get; set; } = gameId;
    public Game Game { get; set; } = null!;
    public int TemplateId = templateId;
    public ProjectTemplate Template { get; set; } = null!;

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Status { get; set; } = "Available";

    public ProjectsOverview ToOverview()
    {
        return new ProjectsOverview(
            Id is null ? 0 : (int) Id,
            Template.Name,
            Template.Rounds,
            Template.Reward
        );
    }
}