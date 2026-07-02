namespace VentureOS.Application.RedTeam.AcceptRedTeamReview;

public sealed record AcceptRedTeamReviewCommand(
    Guid CaseId,
    RedTeamReviewResultDto Review);
