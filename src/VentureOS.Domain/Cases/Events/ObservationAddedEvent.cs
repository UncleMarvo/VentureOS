using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record ObservationAddedEvent(
    Guid CaseId,
    Guid ObservationId,
    string Summary) : DomainEvent;