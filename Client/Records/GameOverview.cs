namespace Client.Records;

// Root object describing a game
public sealed record GameOverview(
    int Id,
    string Name,
    ICollection<PlayerOverview> Players,
    int PlayersCount,
    int MaximumPlayersCount,
    int MaximumRounds,
    int CurrentRound,
    string Status,
    ICollection<RoundOverview> Rounds,
    ICollection<ConsultantOverview> Consultants,
<<<<<<< HEAD
    ICollection<ProjectsOverview> Projects,
    ICollection<EmployeeOverview> Employees
=======
    ICollection<ProjectsOverview> Projects
>>>>>>> 6e737187807dcf3e29d970e7cf9d80ccc63edbd0
);

public sealed record PlayerOverview(
    int Id,
    string Name,
    CompanyOverview Company
);

public sealed record CompanyOverview(
    int Id,
    string Name,
    int Treasury,
    ICollection<EmployeeOverview> Employees,
    ICollection<ProjectsOverview> Projects
);

public record ConsultantOverview(
    int Id,
    string Name,
    int SalaryRequirement,
    ICollection<SkillOverview> Skills
);

public sealed record EmployeeOverview(
    int Id,
    string Name,
<<<<<<< HEAD
    decimal Salary,
=======
    double Salary,
>>>>>>> 6e737187807dcf3e29d970e7cf9d80ccc63edbd0
    ICollection<SkillOverview> Skills
);

public sealed record ProjectsOverview(
    int Id,
    string Name,
    int Rounds,
    double Reward
// ICollection<SkillOverview> Skills
);

public sealed record SkillOverview(
    string Name,
    int Level
);

public sealed record RoundOverview(
    int Id,
    ICollection<RoundActionOverview> Actions
);

public sealed record RoundActionOverview(
    string ActionType,
    string Payload,
    int PlayerId
);
