using VentureOS.Application.Cases.AddObservation;
using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;
using Microsoft.AspNetCore.Mvc;

namespace VentureOS.Api.Endpoints;

public static class ObservationEndpoints
{
    public static IEndpointRouteBuilder MapObservationEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/observations",
            async (
                Guid id,
                [FromBody] AddObservationRequest request,
                [FromServices] AddObservationHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new AddObservationCommand(
                    id,
                    request.ObservationText,
                    request.Summary,
                    request.SourceReference,
                    request.ObservationSource,
                    Confidence.FromPercentage(request.Confidence));

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/observations/{result.Value.ObservationId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record AddObservationRequest(
    string ObservationText,
    string Summary,
    string SourceReference,
    ObservationSource ObservationSource,
    int Confidence);