namespace VentureOS.Application.RedTeam.AcceptRedTeamReview;

public sealed record AcceptedRedTeamReviewDto(
    Guid CaseId,
    int ChallengesCreated);
