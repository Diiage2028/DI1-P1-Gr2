using Server.Hubs.Records;

namespace Server.Models;

public class Project(string name, int gameId, int rounds, double earnings)
{
    public int? Id { get; private set; } // primary key auto increment
    public string Name { get; set; } = name;
    public int GameId { get; set; } = gameId;
    public Game Game { get; set; } = null!;
    public int Rounds { get; set; } = rounds;
    public double Earnings { get; set; } = earnings;

    // Skill with an assigned level
    public ICollection<LeveledSkill> Skills { get; } = [];


    public ProjectsOverview ToOverview()
    {
        return new ProjectsOverview(
            Id is null ? 0 : (int) Id, Name, Rounds, Earnings,
            Skills.Select(s => s.ToOverview()).ToList()
        );
    }
}