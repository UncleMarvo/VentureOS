using VentureOS.Domain.Assumptions;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Evidence;
using VentureOS.Domain.Hypotheses;
using VentureOS.Domain.Opportunities;

namespace VentureOS.Application.Board;

public sealed record BoardDossierDto(
    Guid CaseId,
    string Title,
    string Mission,
    IReadOnlyList<BoardObservationDto> Observations,
    IReadOnlyList<BoardEvidenceDto> SupportingEvidence,
    IReadOnlyList<BoardEvidenceDto> ContradictingEvidence,
    IReadOnlyList<BoardEvidenceDto> NeutralEvidence,
    IReadOnlyList<BoardAssumptionDto> UnresolvedAssumptions,
    IReadOnlyList<BoardHypothesisDto> Hypotheses,
    IReadOnlyList<BoardOpportunityDto> Opportunities,
    IReadOnlyList<BoardChallengeDto> Challenges,
    IReadOnlyList<BoardQualityFindingDto> ResearchQualityFindings,
    IReadOnlyList<BoardQualityFindingDto> RedTeamQualityFindings);

public sealed record BoardObservationDto(
    Guid Id,
    string Summary,
    string SourceReference,
    int Confidence,
    DateTime CreatedAtUtc);

// Evidence carries no Confidence field in the Domain today - only Observations, Assumptions,
// Hypotheses, Opportunities, and Challenges do.
public sealed record BoardEvidenceDto(
    Guid Id,
    string Summary,
    string Interpretation,
    EvidenceDirection Direction,
    DateTime CreatedAtUtc);

// Every Assumption is included here regardless of Status: Assumption has no status-transition
// mutator in the Domain today, so Status is permanently Proposed from creation. "Unresolved"
// currently means "all of them" - this is named honestly rather than faked with a heuristic.
public sealed record BoardAssumptionDto(
    Guid Id,
    string Statement,
    string Rationale,
    AssumptionStatus Status,
    int Confidence,
    DateTime CreatedAtUtc);

public sealed record BoardHypothesisDto(
    Guid Id,
    string Statement,
    string Reasoning,
    HypothesisStatus Status,
    int Confidence,
    DateTime CreatedAtUtc);

public sealed record BoardOpportunityDto(
    Guid Id,
    string Statement,
    string CustomerValue,
    string CommercialValue,
    OpportunityStatus Status,
    int Confidence,
    DateTime CreatedAtUtc);

public sealed record BoardChallengeDto(
    Guid Id,
    ChallengeTarget Target,
    Guid TargetId,
    string TargetText,
    string Statement,
    string Reasoning,
    int Confidence,
    DateTime CreatedAtUtc);

public sealed record BoardQualityFindingDto(
    string Severity,
    string Code,
    string Path,
    string Message);
