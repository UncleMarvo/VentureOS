namespace VentureOS.Application.Board;

public sealed record BoardCaseQuery(
    Guid CaseId,
    IReadOnlyList<BoardQualityFindingDto> ResearchQualityFindings,
    IReadOnlyList<BoardQualityFindingDto> RedTeamQualityFindings);
