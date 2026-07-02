using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record OpportunityCreatedEvent(
    Guid CaseId,
    Guid OpportunityId,
    string Statement) : DomainEvent;
