using System.Text.Json.Serialization;
using Server.Hubs.Records;

namespace Server.Models;

public enum RoundActionType
{
    SendEmployeeForTraining,
    ParticipateInProject,
    EnrollEmployee,
    FireAnEmployee,
    ConfirmRound,
}

[JsonDerivedType(typeof(RoundAction), typeDiscriminator: "DEFAULT")]
[JsonDerivedType(typeof(SendEmployeeForTrainingRoundAction), typeDiscriminator: "SendEmployeeForTraining")] // Added this line
[JsonDerivedType(typeof(ParticipateInProjectRoundAction), typeDiscriminator: "ParticipateInProject")]
[JsonDerivedType(typeof(EnrollEmployeeRoundAction), typeDiscriminator: "EnrollEmployee")]
[JsonDerivedType(typeof(FireAnEmployeeRoundAction), typeDiscriminator: "FireAnEmployee")]
[JsonDerivedType(typeof(ConfirmRoundAction), typeDiscriminator: "ConfirmRound")]
public class RoundAction(int? playerId)
{
    public class RoundActionPayload { }

    public static RoundAction CreateForType(RoundActionType actionType, int? playerId, RoundActionPayload payload)
    {
        RoundAction action = actionType switch
        {
            RoundActionType.SendEmployeeForTraining => new SendEmployeeForTrainingRoundAction(playerId),
            RoundActionType.ParticipateInProject => new ParticipateInProjectRoundAction(playerId),
            RoundActionType.EnrollEmployee => new EnrollEmployeeRoundAction(playerId),
            RoundActionType.FireAnEmployee => new FireAnEmployeeRoundAction(playerId),
            RoundActionType.ConfirmRound => new ConfirmRoundAction(playerId),
            _ => new RoundAction(playerId), // Changed to base class
        };

        action.ApplyPayload(payload);

        return action;
    }

    protected virtual void ApplyPayload(RoundActionPayload payload) { }

    public int? PlayerId { get; init; } = playerId;
    public RoundActionType Type => GetActionType();

    protected virtual RoundActionType GetActionType() => RoundActionType.ConfirmRound; // Default

    public RoundActionOverview ToOverview()
    {
        return new RoundActionOverview(
            Type.ToString(),
            "PAYLOAD", // You'll need to implement this properly
            PlayerId ?? 0
        );
    }
}

// Add the missing class
public class SendEmployeeForTrainingRoundAction(int? playerId) : RoundAction(playerId)
{
    public class SendEmployeeForTrainingPayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public SendEmployeeForTrainingPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (SendEmployeeForTrainingPayload)payload;
    }

    protected override RoundActionType GetActionType() => RoundActionType.SendEmployeeForTraining;
}

public class ParticipateInProjectRoundAction(int? playerId) : RoundAction(playerId)
{
    public class ParticipateInProjectPayload : RoundActionPayload
    {
        public int ProjectId { get; init; }
    }

    public ParticipateInProjectPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (ParticipateInProjectPayload)payload;
    }

    protected override RoundActionType GetActionType() => RoundActionType.ParticipateInProject;
}

public class EnrollEmployeeRoundAction(int? playerId) : RoundAction(playerId)
{
    public class EnrollEmployeeRoundPayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public EnrollEmployeeRoundPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (EnrollEmployeeRoundPayload)payload;
    }

    protected override RoundActionType GetActionType() => RoundActionType.EnrollEmployee;
}

public class FireAnEmployeeRoundAction(int? playerId) : RoundAction(playerId)
{
    public class FireAnEmployeePayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public FireAnEmployeePayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (FireAnEmployeePayload)payload;
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
