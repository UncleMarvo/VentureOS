using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record HypothesisCreatedEvent(
    Guid CaseId,
    Guid HypothesisId,
    string Statement) : DomainEvent;