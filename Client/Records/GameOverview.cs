namespace Client.Records;

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
    ICollection<ProjectsOverview> Projects
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
    int Salary,
    ICollection<SkillOverview> Skills
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

public sealed record ProjectsOverview(
    int Id,
    string Name,
    int Rounds,
    double reward
    // ICollection<SkillOverview> Skills
);