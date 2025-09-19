using FluentResults;
using Server.Actions.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

public sealed record GetStatsParams();

public class GetStatsAction : IAction<GetStatsParams, Result<GameStat>>
{
    private readonly IGameStatsRepository _repo;

    public GetStatsAction(IGameStatsRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<GameStat>> PerformAsync(GetStatsParams actionParams)
    {
        try
        {
            var stats = await _repo.GetStats();
            return Result.Ok(stats);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve statistics: {ex.Message}");
        }
    }
}
