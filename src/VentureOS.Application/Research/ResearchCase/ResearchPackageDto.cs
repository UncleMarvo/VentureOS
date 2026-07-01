namespace VentureOS.Application.Research.ResearchCase;

public sealed record ResearchPackageDto(
    Guid CaseId,
    string Mission,
    IReadOnlyList<ProposedObservationDto> Observations,
    IReadOnlyList<ProposedEvidenceDto> Evidence,
    IReadOnlyList<ProposedAssumptionDto> Assumptions,
    IReadOnlyList<ProposedHypothesisDto> Hypotheses,
    IReadOnlyList<ProposedChallengeDto> Challenges);

public sealed record ProposedObservationDto(
    string ObservationText,
    string Summary,
    string SourceReference,
    int Confidence);

public sealed record ProposedEvidenceDto(
    string Summary,
    string Interpretation,
    int Direction,
    IReadOnlyList<int> ObservationIndexes);

public sealed record ProposedAssumptionDto(
    string Statement,
    string Rationale,
    int Confidence);

public sealed record ProposedHypothesisDto(
    string Statement,
    string Reasoning,
    string ExpectedOutcome,
    string SuccessCriteria,
    int Confidence,
    IReadOnlyList<int> EvidenceIndexes,
    IReadOnlyList<int> AssumptionIndexes);

public sealed record ProposedChallengeDto(
    string Statement,
    string Reasoning,
    int Confidence,
    string TargetType,
    int TargetIndex);
