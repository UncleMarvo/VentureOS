using VentureOS.Domain.Decisions;

namespace VentureOS.Application.Cases.RecordDecision;

public sealed record RecordDecisionResult(
    Guid CaseId,
    Guid DecisionId,
    DecisionOutcome Outcome,
    string Question);