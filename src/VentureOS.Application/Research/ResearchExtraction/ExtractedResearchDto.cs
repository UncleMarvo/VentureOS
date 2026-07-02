using VentureOS.Application.Research.ResearchCase;

namespace VentureOS.Application.Research.ResearchExtraction;

public sealed record ExtractedResearchDto(
    IReadOnlyList<ProposedObservationDto> Observations,
    IReadOnlyList<ProposedEvidenceDto> Evidence,
    IReadOnlyList<ProposedAssumptionDto> Assumptions,
    IReadOnlyList<ProposedOpportunityDto> Opportunities,
    IReadOnlyList<ProposedHypothesisDto> Hypotheses,
    IReadOnlyList<ProposedChallengeDto> Challenges);
