using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.CreateOpportunity;
using VentureOS.Domain.Common;

namespace VentureOS.Api.Endpoints;

public static class OpportunityEndpoints
{
    public static IEndpointRouteBuilder MapOpportunityEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/opportunities",
            async (
                Guid id,
                [FromBody] CreateOpportunityRequest request,
                [FromServices] CreateOpportunityHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateOpportunityCommand(
                    id,
                    request.Statement,
                    request.CustomerValue,
                    request.CommercialValue,
                    request.Differentiation,
                    request.Timing,
                    Confidence.FromPercentage(request.Confidence),
                    request.EvidenceIds,
                    request.AssumptionIds);

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/opportunities/{result.Value.OpportunityId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record CreateOpportunityRequest(
    string Statement,
    string CustomerValue,
    string CommercialValue,
    string Differentiation,
    string Timing,
    int Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);
