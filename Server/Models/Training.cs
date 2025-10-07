using Server.Hubs.Records;

namespace Server.Models;

public class Training()
{

    public int? Id {  get;  init; }
    public string? Name { get;  set; }
    public int Cost { get; set; } 
    public int SkillId { get;  set; }
    public Skill? Skill { get; set; }
    public int NbRound { get; set; }

}
