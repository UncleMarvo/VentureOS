namespace VentureOS.Application.Research.ResearchPlanning;

public sealed record ResearchEvidencePlanDto(
    IReadOnlyList<ResearchQuestionDto> Questions,
    IReadOnlyList<EvidenceNeedDto> EvidenceNeeds);

public sealed record ResearchQuestionDto(
    string Question,
    int Priority);

public sealed record EvidenceNeedDto(
    string Topic,
    string Reason);
