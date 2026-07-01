using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.RaiseChallenge;
using VentureOS.Domain.Challenges;
using VentureOS.Domain.Common;

namespace VentureOS.Api.Endpoints;

public static class ChallengeEndpoints
{
    public static IEndpointRouteBuilder MapChallengeEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/challenges",
            async (
                Guid id,
                [FromBody] RaiseChallengeRequest request,
                [FromServices] RaiseChallengeHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RaiseChallengeCommand(
                    id,
                    request.Target,
                    request.TargetId,
                    request.Statement,
                    request.Reasoning,
                    Confidence.FromPercentage(request.Confidence));

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/challenges/{result.Value.ChallengeId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record RaiseChallengeRequest(
    ChallengeTarget Target,
    Guid TargetId,
    string Statement,
    string Reasoning,
    int Confidence);
