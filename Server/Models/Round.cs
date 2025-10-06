using Server.Hubs.Records;

namespace Server.Models;

public class Round(int gameId, int order)
{
    public int? Id { get; private set; }

    public int GameId { get; init; } = gameId;

    public Game Game { get; } = null!;

    public int Order { get; init; } = order;

    public ICollection<RoundAction> Actions { get; } = [];

    public bool CanPlayerActIn(int PlayerId)
    {
        return !Actions.Any(a => a.PlayerId == PlayerId && a.Type == RoundActionType.ConfirmRound);
    }

    public bool EverybodyPlayed()
    {
        return Game.Players.All(p => Actions.Any(a => a.PlayerId == p.Id && a.Type == RoundActionType.ConfirmRound));
    }

    public RoundOverview ToOverview()
    {
        return new RoundOverview(
            Id: Id ?? 0,
            Actions.Select(a => a.ToOverview()).ToList()
        );
    }
}
