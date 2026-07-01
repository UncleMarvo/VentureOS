using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Application.Decisions.GetDecisionContext;

public sealed record DecisionContextDto(
    DecisionContextDecisionDto Decision,
    IReadOnlyList<DecisionContextEvidenceDto> Evidence,
    IReadOnlyList<DecisionContextAssumptionDto> Assumptions,
    IReadOnlyList<DecisionContextHypothesisDto> Hypotheses,
    IReadOnlyList<DecisionContextChallengeDto> Challenges);

public sealed record DecisionContextDecisionDto(
    Guid Id,
    string Question,
    DecisionOutcome Outcome,
    string Rationale,
    string ExpectedOutcome,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record DecisionContextEvidenceDto(
    Guid Id,
    string Summary,
    string Interpretation,
    DateTime CreatedAtUtc);

public sealed record DecisionContextAssumptionDto(
    Guid Id,
    string Statement,
    string Rationale,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record DecisionContextHypothesisDto(
    Guid Id,
    string Statement,
    string Reasoning,
    Confidence Confidence,
    DateTime CreatedAtUtc);

public sealed record DecisionContextChallengeDto(
    Guid Id,
    string Statement,
    string Reasoning,
    Confidence Confidence,
    DateTime CreatedAtUtc);
