using Microsoft.EntityFrameworkCore;

using Npgsql.PostgresTypes;

using Server.Hubs.Records;

namespace Server.Models;

public class InTraining
{
    public int TrainingId { get; set; }
    public Training? Training { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int StartRoundId { get; set; }
    public Round? Round { get; set; }
}


