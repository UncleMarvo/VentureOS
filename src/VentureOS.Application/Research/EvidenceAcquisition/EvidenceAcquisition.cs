namespace VentureOS.Application.Research.EvidenceAcquisition;

public sealed record EvidenceAcquisitionResultDto(
    IReadOnlyList<AcquiredEvidenceDto> Evidence);

public sealed record AcquiredEvidenceDto(
    string ResearchQuestion,
    string Findings,
    string SourceReference,
    int Confidence,
    IReadOnlyList<string> Unknowns);
