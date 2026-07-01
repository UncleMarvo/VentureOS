namespace VentureOS.Application.Cases.GetCaseBrief;

public sealed record CaseBriefDto(
    Guid CaseId,
    string Title,
    string Mission,
    CaseBriefDecisionDto? LatestDecision,
    CaseBriefLessonDto? LatestLesson,
    CaseBriefCountsDto Counts,
    DateTime? LastActivityAtUtc);

public sealed record CaseBriefDecisionDto(
    Guid Id,
    string Question,
    string Outcome,
    string Rationale,
    DateTime CreatedAtUtc);

public sealed record CaseBriefLessonDto(
    Guid Id,
    string Summary,
    string Detail,
    DateTime CreatedAtUtc);

public sealed record CaseBriefCountsDto(
    int Observations,
    int Evidence,
    int Assumptions,
    int Hypotheses,
    int Challenges,
    int Decisions,
    int Lessons);
