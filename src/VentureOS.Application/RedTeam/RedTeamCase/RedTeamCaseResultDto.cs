using VentureOS.Application.RedTeam.RedTeamQuality;

namespace VentureOS.Application.RedTeam.RedTeamCase;

public sealed record RedTeamCaseResultDto(
    RedTeamReviewResultDto Review,
    IReadOnlyList<RedTeamQualityIssueDto> QualityIssues);
