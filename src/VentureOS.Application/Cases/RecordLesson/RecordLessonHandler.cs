using VentureOS.Domain.Common;
using VentureOS.Domain.Lessons;

namespace VentureOS.Application.Cases.RecordLesson;

public sealed class RecordLessonHandler
{
    private readonly ICaseRepository _caseRepository;

    public RecordLessonHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<Result<RecordLessonResult>> HandleAsync(
        RecordLessonCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ventureCase = await _caseRepository.GetByIdAsync(
            command.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<RecordLessonResult>.Failure("Case not found.");
        }

        var draft = new LessonDraft(
            command.Summary,
            command.Detail,
            command.Confidence,
            command.DecisionIds);

        var recordLessonResult = ventureCase.RecordLesson(draft);

        if (recordLessonResult.IsFailure)
        {
            return Result<RecordLessonResult>.Failure(
                recordLessonResult.Error ?? "Lesson could not be recorded.");
        }

        var lesson = recordLessonResult.Value;

        await _caseRepository.UpdateAsync(
            ventureCase,
            cancellationToken);

        return Result<RecordLessonResult>.Success(
            new RecordLessonResult(
                ventureCase.Id,
                lesson.Id,
                lesson.Summary));
    }
}