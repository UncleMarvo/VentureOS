using VentureOS.Domain.Common;

namespace VentureOS.Domain.Cases.Events;

public sealed record LessonRecordedEvent(
    Guid CaseId,
    Guid LessonId,
    string Summary) : DomainEvent;