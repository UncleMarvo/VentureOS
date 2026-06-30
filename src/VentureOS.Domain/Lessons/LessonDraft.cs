using VentureOS.Domain.Common;

namespace VentureOS.Domain.Lessons;

public sealed record LessonDraft(
    string Summary,
    string Detail,
    Confidence Confidence,
    IReadOnlyCollection<Guid> DecisionIds);