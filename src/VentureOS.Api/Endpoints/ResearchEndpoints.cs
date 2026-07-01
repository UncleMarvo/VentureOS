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

        return app;
    }
}
