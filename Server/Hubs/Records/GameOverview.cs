namespace Server.Hubs.Records;
//
// This file defines a bunch of record types (immutable DTOs) that together describe the state of a game.
// all the overview records that the server sends to the clients via SignalR (or API).
// DTO (data transfer object) optimized for serialiazation to be sent, for communication
//

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
    ICollection<EmployeeOverview> Employees
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
    decimal Salary,
    ICollection<SkillOverview> Skills
);

public sealed record ProjectsOverview(
    int Id,
    string Name,
    int Rounds,
    double Earnings,
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
