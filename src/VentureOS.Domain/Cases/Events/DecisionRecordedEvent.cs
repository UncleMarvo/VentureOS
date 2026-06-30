using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Domain.Cases.Events;

public sealed record DecisionRecordedEvent(
    Guid CaseId,
    Guid DecisionId,
    DecisionOutcome Outcome,
    string Question) : DomainEvent;