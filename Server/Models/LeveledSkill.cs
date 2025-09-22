using Server.Hubs.Records;

namespace Server.Models;

// DTO (data transfer object) or helper class that pairs a skill’s name with a level.
public class LeveledSkill(string name, int level)
{
    public string Name { get; set; } = name;

    public int Level { get; set; } = level;

    public SkillOverview ToOverview()
    {
        return new SkillOverview(Name, Level);
    }
}
