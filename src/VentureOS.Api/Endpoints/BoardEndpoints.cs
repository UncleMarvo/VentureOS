using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Board;

namespace VentureOS.Api.Endpoints;

public static class BoardEndpoints
{
    public static IEndpointRouteBuilder MapBoardEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{caseId:guid}/board/review",
            async (
                Guid caseId,
                [FromBody] BoardReviewRequest? request,
                [FromServices] BoardCaseHandler handler,
                CancellationToken cancellationToken) =>
            {
                var query = new BoardCaseQuery(
                    caseId,
                    MapFindings(request?.ResearchQualityFindings),
                    MapFindings(request?.RedTeamQualityFindings));

                var result = await handler.HandleAsync(query, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            });

        return app;
    }

    private static IReadOnlyList<BoardQualityFindingDto> MapFindings(
        IReadOnlyList<BoardQualityFindingRequest>? findings)
    {
        return findings is null
            ? []
            : findings
                .Select(finding => new BoardQualityFindingDto(
                    finding.Severity,
                    finding.Code,
                    finding.Path,
                    finding.Message))
                .ToList();
    }
}

public sealed record BoardReviewRequest(
    IReadOnlyList<BoardQualityFindingRequest>? ResearchQualityFindings,
    IReadOnlyList<BoardQualityFindingRequest>? RedTeamQualityFindings);

public sealed record BoardQualityFindingRequest(
    string Severity,
    string Code,
    string Path,
    string Message);
