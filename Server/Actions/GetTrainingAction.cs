using FluentResults;

using Server.Actions.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

public sealed record GetTrainingParams();

public class GetTrainingAction : IAction<GetTrainingParams, Result<List<Training>>>
{
    private readonly ITrainingRepository _repo;

    public GetTrainingAction(ITrainingRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<Training>>> PerformAsync(GetTrainingParams actionParams)
    {
        try
        {
            var trainings = await _repo.GetTrainings();
            return Result.Ok(trainings);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve trainings: {ex.Message}");
        }
    }
}
