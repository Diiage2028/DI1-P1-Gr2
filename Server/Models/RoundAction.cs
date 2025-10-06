using System.Text.Json.Serialization;
using Server.Hubs.Records;

namespace Server.Models;

public enum RoundActionType
{
    Default,
    SendEmployeeForTraining,
    ParticipateInProject,
    EnrollEmployee,
    FireAnEmployee,
    ConfirmRound,
}

public class RoundActionPayload { }

[JsonDerivedType(typeof(RoundAction), typeDiscriminator: "DEFAULT")]
[JsonDerivedType(typeof(SendEmployeeForTrainingRoundAction), typeDiscriminator: "SendEmployeeForTraining")]
[JsonDerivedType(typeof(ParticipateInProjectRoundAction), typeDiscriminator: "ParticipateInProject")]
[JsonDerivedType(typeof(EnrollEmployeeRoundAction), typeDiscriminator: "EnrollEmployee")]
[JsonDerivedType(typeof(FireAnEmployeeRoundAction), typeDiscriminator: "FireAnEmployee")]
[JsonDerivedType(typeof(ConfirmRoundAction), typeDiscriminator: "ConfirmRound")]
public class RoundAction(int? playerId)
{
    public static RoundAction CreateForType(RoundActionType actionType, int? playerId, RoundActionPayload payload)
    {
        RoundAction action = actionType switch
        {
            RoundActionType.SendEmployeeForTraining => new SendEmployeeForTrainingRoundAction(playerId),
            RoundActionType.ParticipateInProject => new ParticipateInProjectRoundAction(playerId),
            RoundActionType.EnrollEmployee => new EnrollEmployeeRoundAction(playerId),
            RoundActionType.FireAnEmployee => new FireAnEmployeeRoundAction(playerId),
            RoundActionType.ConfirmRound => new ConfirmRoundAction(playerId),
            _ => throw new ArgumentException($"Unknown action type: {actionType}")
        };

        action.ApplyPayload(payload);

        return action;
    }

    protected virtual void ApplyPayload(RoundActionPayload payload)
    {
        // Base implementation does nothing
    }

    public int Id { get; set; }
    public int RoundId { get; set; }
    public int? PlayerId { get; init; } = playerId;
    public RoundActionType Type => GetActionType();

    protected virtual RoundActionType GetActionType() => RoundActionType.Default;

    public RoundActionOverview ToOverview()
    {
        return new RoundActionOverview(
            Type.ToString(),
            "PAYLOAD",
            PlayerId ?? 0
        );
    }
}

public class SendEmployeeForTrainingRoundAction(int? playerId) : RoundAction(playerId)
{
    [JsonDerivedType(typeof(SendEmployeeForTrainingPayload), typeDiscriminator: "SendEmployeeForTraining")]
    public class SendEmployeeForTrainingPayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public SendEmployeeForTrainingPayload Payload { get; private set; } = new();

    protected override void ApplyPayload(RoundActionPayload payload) => Payload = (SendEmployeeForTrainingPayload)payload;

    protected override RoundActionType GetActionType() => RoundActionType.SendEmployeeForTraining;
}

public class ParticipateInProjectRoundAction(int? playerId) : RoundAction(playerId)
{
    [JsonDerivedType(typeof(ParticipateInProjectPayload), typeDiscriminator: "ParticipateInProject")]
    public class ParticipateInProjectPayload : RoundActionPayload
    {
        public int ProjectId { get; init; }
    }

    public ParticipateInProjectPayload Payload { get; private set; } = new();

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        if (payload is ParticipateInProjectPayload specificPayload)
        {
            Payload = specificPayload;
        }
        else
        {
            throw new InvalidCastException($"Expected ParticipateInProjectPayload but got {payload.GetType().Name}");
        }
    }

    protected override RoundActionType GetActionType() => RoundActionType.ParticipateInProject;
}

public class EnrollEmployeeRoundAction(int? playerId) : RoundAction(playerId)
{
    protected override void ApplyPayload(RoundActionPayload payload)
    {
    }
}

public class FireAnEmployeeRoundAction(int? playerId) : RoundAction(playerId)
{
    [JsonDerivedType(typeof(FireAnEmployeePayload), typeDiscriminator: "FireAnEmployee")]
    public class FireAnEmployeePayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public FireAnEmployeePayload Payload { get; private set; } = new();

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        if (payload is FireAnEmployeePayload specificPayload)
        {
            Payload = specificPayload;
        }
        else
        {
            throw new InvalidCastException($"Expected FireAnEmployeePayload but got {payload.GetType().Name}");
        }
    }

    protected override RoundActionType GetActionType() => RoundActionType.FireAnEmployee;
}

public class ConfirmRoundAction(int? playerId) : RoundAction(playerId)
{
    protected override void ApplyPayload(RoundActionPayload payload)
    {
        // No payload for confirm action
    }

    protected override RoundActionType GetActionType() => RoundActionType.ConfirmRound;
}
