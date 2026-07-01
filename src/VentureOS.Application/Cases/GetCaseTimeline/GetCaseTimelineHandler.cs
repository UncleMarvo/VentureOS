using VentureOS.Application.Common;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.GetCaseTimeline;

public sealed class GetCaseTimelineHandler
{
    private readonly ICaseRepository _caseRepository;

    public GetCaseTimelineHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<CaseTimelineDto>> HandleAsync(
        GetCaseTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<CaseTimelineDto>.Failure("Case not found.");
        }

        var items = new List<CaseTimelineItemDto>();

        // ========================
        // OBSERVATION
        // ========================
        items.AddRange(
            ventureCase.Observations.Select(observation =>
                new CaseTimelineItemDto(
                    observation.Id,
                    CaseTimelineItemType.Observation,
                    observation.ObservationText,
                    null,
                    observation.CreatedAtUtc)));

        // ========================
        // EVIDENCE
        // ========================
        items.AddRange(
            ventureCase.Evidence.Select(evidence =>
                new CaseTimelineItemDto(
                    evidence.Id,
                    CaseTimelineItemType.Evidence,
                    evidence.Summary,
                    evidence.Interpretation,
                    evidence.CreatedAtUtc)));

        // ========================
        // ASSUMPTION
        // ========================
        items.AddRange(
            ventureCase.Assumptions.Select(assumption =>
                new CaseTimelineItemDto(
                    assumption.Id,
                    CaseTimelineItemType.Assumption,
                    assumption.Statement,
                    assumption.Rationale,
                    assumption.CreatedAtUtc)));

        // ========================
        // HYPOTHESIS
        // ========================
        items.AddRange(
            ventureCase.Hypotheses.Select(hypothesis =>
                new CaseTimelineItemDto(
                    hypothesis.Id,
                    CaseTimelineItemType.Hypothesis,
                    hypothesis.Statement,
                    hypothesis.Reasoning,
                    hypothesis.CreatedAtUtc)));

        // ========================
        // CHALENGE
        // ========================
        items.AddRange(
            ventureCase.Challenges.Select(challenge =>
                new CaseTimelineItemDto(
                    challenge.Id,
                    CaseTimelineItemType.Challenge,
                    challenge.Statement,
                    challenge.Reasoning,
                    challenge.CreatedAtUtc)));

        // ========================
        // DECISION
        // ========================
        items.AddRange(
            ventureCase.Decisions.Select(decision =>
                new CaseTimelineItemDto(
                    decision.Id,
                    CaseTimelineItemType.Decision,
                    decision.Question,
                    decision.Rationale,
                    decision.CreatedAtUtc)));

        // ========================
        // LESSON
        // ========================
        items.AddRange(
            ventureCase.Lessons.Select(lesson =>
                new CaseTimelineItemDto(
                    lesson.Id,
                    CaseTimelineItemType.Lesson,
                    lesson.Summary,
                    lesson.Detail,
                    lesson.CreatedAtUtc)));

        var timeline = items
            .OrderBy(item => item.CreatedAtUtc)
            .ToList();

        return Result<CaseTimelineDto>.Success(
            new CaseTimelineDto(
                ventureCase.Id,
                timeline));
    }
}
