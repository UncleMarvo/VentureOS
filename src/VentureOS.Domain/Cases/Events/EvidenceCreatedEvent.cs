using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;

namespace VentureOS.Domain.Cases.Events;

public sealed record EvidenceCreatedEvent(
    Guid CaseId,
    Guid EvidenceId,
    EvidenceDirection Direction,
    string Summary) : DomainEvent;