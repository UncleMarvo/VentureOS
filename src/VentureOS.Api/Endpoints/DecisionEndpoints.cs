using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.RecordDecision;
using VentureOS.Application.Decisions.GetDecisionContext;
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

        app.MapGet(
            "/cases/{caseId:guid}/decisions/{decisionId:guid}/context",
            async (
                Guid caseId,
                Guid decisionId,
                GetDecisionContextHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetDecisionContextQuery(caseId, decisionId),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
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
