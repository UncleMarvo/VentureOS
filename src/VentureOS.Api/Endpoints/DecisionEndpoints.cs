using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.RecordDecision;
using VentureOS.Domain.Common;
using VentureOS.Domain.Decisions;

namespace VentureOS.Api.Endpoints;

public static class DecisionEndpoints
{
    public static IEndpointRouteBuilder MapDecisionEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/decisions",
            async (
                Guid id,
                [FromBody] RecordDecisionRequest request,
                [FromServices] RecordDecisionHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RecordDecisionCommand(
                    id,
                    request.Question,
                    request.Outcome,
                    request.Rationale,
                    request.ExpectedOutcome,
                    Confidence.FromPercentage(request.Confidence),
                    request.EvidenceIds,
                    request.AssumptionIds,
                    request.HypothesisIds,
                    request.ChallengeIds);

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/decisions/{result.Value.DecisionId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record RecordDecisionRequest(
    string Question,
    DecisionOutcome Outcome,
    string Rationale,
    string ExpectedOutcome,
    int Confidence,
    IReadOnlyCollection<Guid> EvidenceIds,
    IReadOnlyCollection<Guid> AssumptionIds,
    IReadOnlyCollection<Guid> HypothesisIds,
    IReadOnlyCollection<Guid> ChallengeIds);
