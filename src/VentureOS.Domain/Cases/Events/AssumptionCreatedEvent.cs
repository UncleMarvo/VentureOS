using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record AssumptionCreatedEvent(
    Guid CaseId,
    Guid AssumptionId,
    string Statement) : DomainEvent;