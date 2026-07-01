using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Research.ResearchCase;

namespace VentureOS.Api.Endpoints;

public static class ResearchEndpoints
{
    public static IEndpointRouteBuilder MapResearchEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{caseId:guid}/research/deep-dive",
            async (
                Guid caseId,
                [FromServices] ResearchCaseHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new ResearchCaseQuery(caseId),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            });

        app.MapPost(
            "/cases/{caseId:guid}/research/accept",
            async (
                Guid caseId,
                [FromBody] ResearchPackageDto package,
                [FromServices] AcceptResearchPackageHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new AcceptResearchPackageCommand(caseId, package),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

        return app;
    }
}
