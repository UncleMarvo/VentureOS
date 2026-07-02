namespace VentureOS.Application.Research.ResearchCase;

public sealed record ResearchPackageDto(
    Guid CaseId,
    string Mission,
    ResearchGenerationDto Generation,
    IReadOnlyList<ProposedObservationDto> Observations,
    IReadOnlyList<ProposedEvidenceDto> Evidence,
    IReadOnlyList<ProposedAssumptionDto> Assumptions,
    IReadOnlyList<ProposedOpportunityDto> Opportunities,
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

public sealed record ProposedOpportunityDto(
    string Statement,
    string CustomerValue,
    string CommercialValue,
    string Differentiation,
    string Timing,
    int Confidence,
    IReadOnlyList<int> EvidenceIndexes,
    IReadOnlyList<int> AssumptionIndexes);

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

public sealed record ResearchGenerationDto(
    string Provider,
    string Model,
    string Persona,
    string PersonaVersion,
    string PromptVersion,
    DateTime GeneratedAtUtc,
    TimeSpan Duration,
    string Limitations);
