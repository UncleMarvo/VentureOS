using VentureOS.Application.Common;

namespace VentureOS.Application.Cases.GetCaseTimeline;

public sealed record GetCaseTimelineQuery(Guid CaseId);

public sealed record CaseTimelineDto(
    Guid CaseId,
    IReadOnlyList<CaseTimelineItemDto> Items);

public sealed record CaseTimelineItemDto(
    Guid Id,
    CaseTimelineItemType Type,
    string Headline,
    string? Detail,
    DateTime CreatedAtUtc);
