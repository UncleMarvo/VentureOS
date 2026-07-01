using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.CreateAssumption;
using VentureOS.Domain.Common;

namespace VentureOS.Api.Endpoints;

public static class AssumptionEndpoints
{
    public static IEndpointRouteBuilder MapAssumptionEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/assumptions",
            async (
                Guid id,
                [FromBody] CreateAssumptionRequest request,
                [FromServices] CreateAssumptionHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateAssumptionCommand(
                    id,
                    request.Statement,
                    request.Rationale,
                    Confidence.FromPercentage(request.Confidence));

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/assumptions/{result.Value.AssumptionId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record CreateAssumptionRequest(
    string Statement,
    string Rationale,
    int Confidence);
