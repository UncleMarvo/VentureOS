using VentureOS.Domain.Common;

namespace VentureOS.Application.Cases.RecordLesson;

public sealed record RecordLessonCommand(
    Guid CaseId,
    string Summary,
    string Detail,
    Confidence Confidence,
    IReadOnlyCollection<Guid> DecisionIds);