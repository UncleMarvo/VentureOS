using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.GetCaseBrief;

public sealed class GetCaseBriefHandler
{
    private readonly ICaseRepository _caseRepository;

    public GetCaseBriefHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<CaseBriefDto>> HandleAsync(
        GetCaseBriefQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CaseBriefDto>.Failure("Case not found.");
        }

        // =======================
        // DECISION
        // =======================
        var latestDecision = ventureCase.Decisions
            .OrderByDescending(decision => decision.CreatedAtUtc)
            .Select(decision =>
                new CaseBriefDecisionDto(
                    decision.Id,
                    decision.Question,
                    decision.Outcome.ToString(),
                    decision.Rationale,
                    decision.CreatedAtUtc))
            .FirstOrDefault();

        // =======================
        // LESSON
        // =======================
        var latestLesson = ventureCase.Lessons
            .OrderByDescending(lesson => lesson.CreatedAtUtc)
            .Select(lesson =>
                new CaseBriefLessonDto(
                    lesson.Id,
                    lesson.Summary,
                    lesson.Detail,
                    lesson.CreatedAtUtc))
            .FirstOrDefault();

        // =======================
        // LAST ACTIVITY AT UTC
        // =======================
        var lastActivityAtUtc = new DateTime?[]
        {
            ventureCase.Observations.Select(observation => observation.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Evidence.Select(evidence => evidence.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Assumptions.Select(assumption => assumption.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Hypotheses.Select(hypothesis => hypothesis.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Challenges.Select(challenge => challenge.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Decisions.Select(decision => decision.CreatedAtUtc).DefaultIfEmpty().Max(),
            ventureCase.Lessons.Select(lesson => lesson.CreatedAtUtc).DefaultIfEmpty().Max()
        }
        .Where(date => date.HasValue)
        .Max();

        var dto = new CaseBriefDto(
            ventureCase.Id,
            ventureCase.Title,
            ventureCase.Mission,
            latestDecision,
            latestLesson,
            new CaseBriefCountsDto(
                ventureCase.Observations.Count,
                ventureCase.Evidence.Count,
                ventureCase.Assumptions.Count,
                ventureCase.Hypotheses.Count,
                ventureCase.Challenges.Count,
                ventureCase.Decisions.Count,
                ventureCase.Lessons.Count),
            lastActivityAtUtc);

        return Result<CaseBriefDto>.Success(dto);
    }
}
