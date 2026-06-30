
using VentureOS.Application.Cases.CreateEvidence;
using VentureOS.Domain.Evidence;
using Microsoft.AspNetCore.Mvc;

namespace VentureOS.Api.Endpoints;

public static class EvidenceEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/evidence",
            async (
                Guid id,
                [FromBody] CreateEvidenceRequest request,
                [FromServices] CreateEvidenceHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateEvidenceCommand(
                    id,
                    request.Summary,
                    request.Interpretation,
                    request.Direction,
                    request.ObservationIds);

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/evidence/{result.Value.EvidenceId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record CreateEvidenceRequest(
    string Summary,
    string Interpretation,
    EvidenceDirection Direction,
    IReadOnlyCollection<Guid> ObservationIds);