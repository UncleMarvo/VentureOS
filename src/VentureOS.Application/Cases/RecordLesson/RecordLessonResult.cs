namespace VentureOS.Application.Cases.RecordLesson;

public sealed record RecordLessonResult(
    Guid CaseId,
    Guid LessonId,
    string Summary);