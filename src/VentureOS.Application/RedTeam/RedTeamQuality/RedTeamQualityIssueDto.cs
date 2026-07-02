namespace VentureOS.Application.RedTeam.RedTeamQuality;

public sealed record RedTeamQualityIssueDto(
    RedTeamQualitySeverity Severity,
    RedTeamQualityCode Code,
    string Path,
    string Message);
