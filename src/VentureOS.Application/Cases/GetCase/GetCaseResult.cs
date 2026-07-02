using VentureOS.Domain.Cases;

namespace VentureOS.Application.Cases.GetCase;

public sealed record GetCaseResult(
    Guid Id,
    string Title,
    string Mission,
    CaseStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int ObservationCount,
    int EvidenceCount,
    int AssumptionCount,
    int OpportunityCount,
    int HypothesisCount,
    int ChallengeCount,
    int DecisionCount,
    int LessonCount);