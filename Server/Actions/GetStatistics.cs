using FluentResults;

using Server.Actions.Contracts;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record GetStatisticsParams();

public sealed record StatisticsResult(int TotalGames, int TotalPlayers);

public class GetStatistics(
    IStatisticsRepository statisticsRepository
) : IAction<GetStatisticsParams, Result<StatisticsResult>>
{
    public async Task<Result<StatisticsResult>> PerformAsync(GetStatisticsParams actionParams)
    {
        var totalGames = await statisticsRepository.GetTotalGamesAsync();
        var totalPlayers = await statisticsRepository.GetTotalPlayersAsync();

        var result = new StatisticsResult(totalGames, totalPlayers);
        return Result.Ok(result);
    }
}
