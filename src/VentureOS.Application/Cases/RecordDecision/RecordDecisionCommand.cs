using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Application.Cases.RecordDecision;

public sealed record RecordDecisionCommand(
    Guid CaseId,
    string Question,
    DecisionOutcome Outcome,
    string Rationale,
    string ExpectedOutcome,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds,
    IReadOnlyCollection<Guid> HypothesisIds,
    IReadOnlyCollection<Guid> ChallengeIds);