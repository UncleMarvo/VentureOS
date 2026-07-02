namespace VentureOS.Application.Research.ResearchCase;
using VentureOS.Application.Research.ResearchQuality;

public sealed record ResearchCaseResultDto(
    ResearchPackageDto ResearchPackage,
    IReadOnlyList<ResearchQualityIssueDto> QualityIssues);
