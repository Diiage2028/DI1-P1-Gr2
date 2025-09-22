using System.Text.Json.Serialization;

using Server.Hubs.Records;

namespace Server.Models;

public enum RoundActionType
{
    SendEmployeeForTraining,
    ParticipateInCallForTenders,
    RecruitAConsultant,
    FireAnEmployee,
    ConfirmRound,
    GenerateNewConsultant,
}

[JsonDerivedType(typeof(RoundAction), typeDiscriminator: "DEFAULT")]
[JsonDerivedType(typeof(SendEmployeeForTrainingRoundAction), typeDiscriminator: "SendEmployeeForTraining")]
[JsonDerivedType(typeof(ParticipateInCallForTendersRoundAction), typeDiscriminator: "ParticipateInCallForTenders")]
[JsonDerivedType(typeof(RecruitAConsultantRoundAction), typeDiscriminator: "RecruitAConsultant")]
[JsonDerivedType(typeof(FireAnEmployeeRoundAction), typeDiscriminator: "FireAnEmployee")]
[JsonDerivedType(typeof(ConfirmRoundAction), typeDiscriminator: "ConfirmRound")]
[JsonDerivedType(typeof(GenerateNewConsultantRoundAction), typeDiscriminator: "GenerateNewConsultant")]
public class RoundAction(int? playerId)
{
    public class RoundActionPayload { }

    public static RoundAction CreateForType(RoundActionType actionType, int? playerId, RoundActionPayload payload)
    {
        RoundAction action = actionType switch
        {
            RoundActionType.SendEmployeeForTraining => new SendEmployeeForTrainingRoundAction(playerId),
            RoundActionType.ParticipateInCallForTenders => new ParticipateInCallForTendersRoundAction(playerId),
            RoundActionType.RecruitAConsultant => new RecruitAConsultantRoundAction(playerId),
            RoundActionType.FireAnEmployee => new FireAnEmployeeRoundAction(playerId),
            RoundActionType.ConfirmRound => new ConfirmRoundAction(playerId),
            _ => new ConfirmRoundAction(playerId),
        };

        action.ApplyPayload(payload);

        return action;
    }

    protected virtual void ApplyPayload(RoundActionPayload payload) { }

    public int? PlayerId { get; init; } = playerId;

    public RoundActionOverview ToOverview()
    {
        return new RoundActionOverview(
            "TYPE", "PAYLOAD", (int) PlayerId!
        );
    }
}

public class SendEmployeeForTrainingRoundAction(int? playerId) : RoundAction(playerId)
{
    public class SendEmployeeForTrainingPayload : RoundActionPayload
    {
        public int EmployeeId { get; init; }
    }

    public SendEmployeeForTrainingPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (SendEmployeeForTrainingPayload) payload;
    }
}

public class ParticipateInCallForTendersRoundAction(int? playerId) : RoundAction(playerId)
{
    public class ParticipateInCallForTendersPayload : RoundActionPayload
    {
        public int CallForTendersId { get; init; }
    }

    public ParticipateInCallForTendersPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (ParticipateInCallForTendersPayload) payload;
    }
}

public class RecruitAConsultantRoundAction(int? playerId) : RoundAction(playerId)
{
    public class RecruitAConsultantPayload : RoundActionPayload
    {
        public int ConsultantId { get; init; }
    }

    public RecruitAConsultantPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (RecruitAConsultantPayload) payload;
    }
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
        Payload = (FireAnEmployeePayload) payload;
    }
}

public class ConfirmRoundAction(int? playerId) : RoundAction(playerId)
{
    protected override void ApplyPayload(RoundActionPayload payload)
    {
    }
}

public class GenerateNewConsultantRoundAction(int? gameId) : RoundAction(gameId)
{
    public class GenerateNewConsultantPayload : RoundActionPayload
    {
        public int GameId { get; init; }
    }

    public GenerateNewConsultantPayload Payload { get; private set; } = null!;

    protected override void ApplyPayload(RoundActionPayload payload)
    {
        Payload = (GenerateNewConsultantPayload) payload;
    }
}
