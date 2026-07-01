using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.CreateHypothesis;
using VentureOS.Domain.Common;

namespace VentureOS.Api.Endpoints;

public static class HypothesisEndpoints
{
    public static IEndpointRouteBuilder MapHypothesisEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/hypotheses",
            async (
                Guid id,
                [FromBody] CreateHypothesisRequest request,
                [FromServices] CreateHypothesisHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateHypothesisCommand(
                    id,
                    request.Statement,
                    request.Reasoning,
                    request.ExpectedOutcome,
                    request.SuccessCriteria,
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
                    $"/cases/{result.Value.CaseId}/hypotheses/{result.Value.HypothesisId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record CreateHypothesisRequest(
    string Statement,
    string Reasoning,
    string ExpectedOutcome,
    string SuccessCriteria,
    int Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds);
