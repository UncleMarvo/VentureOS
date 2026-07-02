namespace VentureOS.Application.Research.ResearchQuality;

public sealed record ResearchQualityIssueDto(
    ResearchQualitySeverity Severity,
    ResearchQualityCode Code,
    string Path,
    string Message);
