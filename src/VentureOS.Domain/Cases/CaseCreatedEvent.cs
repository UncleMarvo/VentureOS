using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record CaseCreatedEvent(Guid CaseId, string Title) : DomainEvent;