using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record ChallengeRaisedEvent(
    Guid CaseId,
    Guid ChallengeId,
    ChallengeTarget Target,
    Guid TargetId,
    string Statement) : DomainEvent;