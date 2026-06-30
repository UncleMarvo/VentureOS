using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Observations;

namespace VentureOS.Domain.Cases;

public sealed record CaseRehydrationState(
    Guid Id,
    string Title,
    string Mission,
    CaseStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<ObservationState> Observations,
    IReadOnlyCollection<EvidenceState> Evidence,
    IReadOnlyCollection<AssumptionState> Assumptions,
    IReadOnlyCollection<HypothesisState> Hypotheses,
    IReadOnlyCollection<ChallengeState> Challenges,
    IReadOnlyCollection<DecisionState> Decisions,
    IReadOnlyCollection<LessonState> Lessons);

public sealed record ObservationState(
    Guid Id,
    Guid CaseId,
    string ObservationText,
    string Summary,
    string SourceReference,
    ObservationSource ObservationSource,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record EvidenceState(
    Guid Id,
    Guid CaseId,
    string Summary,
    string Interpretation,
    EvidenceDirection Direction,
    IReadOnlyCollection<Guid> ObservationIds,
    DateTime CreatedAtUtc);

public sealed record AssumptionState(
    Guid Id,
    Guid CaseId,
    string Statement,
    string Rationale,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record HypothesisState(
    Guid Id,
    Guid CaseId,
    string Statement,
    string Reasoning,
    string ExpectedOutcome,
    string SuccessCriteria,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds,
    DateTime CreatedAtUtc);

public sealed record ChallengeState(
    Guid Id,
    Guid CaseId,
    ChallengeTarget Target,
    Guid TargetId,
    string Statement,
    string Reasoning,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record DecisionState(
    Guid Id,
    Guid CaseId,
    string Question,
    DecisionOutcome Outcome,
    string Rationale,
    string ExpectedOutcome,
    Confidence Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds,
    IReadOnlyCollection<Guid> HypothesisIds,
    IReadOnlyCollection<Guid> ChallengeIds,
    DateTime CreatedAtUtc);

public sealed record LessonState(
    Guid Id,
    Guid CaseId,
    string Summary,
    string Detail,
    Confidence Confidence,
    IReadOnlyCollection<Guid> DecisionIds,
    DateTime CreatedAtUtc);