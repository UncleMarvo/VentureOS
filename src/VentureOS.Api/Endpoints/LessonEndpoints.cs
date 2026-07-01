using Microsoft.AspNetCore.Mvc;
using VentureOS.Application.Cases.RecordLesson;
using VentureOS.Domain.Common;

namespace VentureOS.Api.Endpoints;

public static class LessonEndpoints
{
    public static IEndpointRouteBuilder MapLessonEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases/{id:guid}/lessons",
            async (
                Guid id,
                [FromBody] RecordLessonRequest request,
                [FromServices] RecordLessonHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RecordLessonCommand(
                    id,
                    request.Summary,
                    request.Detail,
                    Confidence.FromPercentage(request.Confidence),
                    request.DecisionIds);

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}/lessons/{result.Value.LessonId}",
                    result.Value);
            });

        return app;
    }
}

public sealed record RecordLessonRequest(
    string Summary,
    string Detail,
    int Confidence,
    IReadOnlyCollection<Guid> DecisionIds);
