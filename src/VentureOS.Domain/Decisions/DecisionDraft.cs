using VentureOS.Domain.Common;

namespace VentureOS.Domain.Decisions;

public sealed record DecisionDraft(
    string Question,
    DecisionOutcome Outcome,
    string Rationale,
    string ExpectedOutcome,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds,
    IReadOnlyCollection<Guid> HypothesisIds,
    IReadOnlyCollection<Guid> ChallengeIds);