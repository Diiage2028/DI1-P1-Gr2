using Server.Hubs.Records;

namespace Server.Models;

public class ProjectTemplate(string name, int rounds, double reward)
{
    public int? Id { get; init; }
    public string Name { get; set; } = name;
    public int Rounds { get; set; } = rounds;
    public double Reward { get; set; } = reward;

    // // Skill with an assigned level
    // public ICollection<LeveledSkill> Skills { get; } = [];
}