using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.RedTeam;
using VentureOS.Application.RedTeam.AcceptRedTeamReview;
using VentureOS.Application.RedTeam.RedTeamCase;

namespace VentureOS.Api.Endpoints;

public static class RedTeamEndpoints
{
    public static IEndpointRouteBuilder MapRedTeamEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{caseId:guid}/red-team/review",
            async (
                Guid caseId,
                [FromServices] RedTeamCaseHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new RedTeamCaseQuery(caseId),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            });

        app.MapPost(
            "/cases/{caseId:guid}/red-team/accept",
            async (
                Guid caseId,
                [FromBody] RedTeamReviewResultDto review,
                [FromServices] AcceptRedTeamReviewHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new AcceptRedTeamReviewCommand(caseId, review),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

        return app;
    }
}
