using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.GetCase;

namespace VentureOS.Api.Endpoints;

public static class CaseEndpoints
{
    public static IEndpointRouteBuilder MapCaseEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/cases",
            async (
                CreateCaseCommand command,
                CreateCaseHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/cases/{result.Value.CaseId}",
                    result.Value);
            });

        app.MapGet(
            "/cases/{id:guid}",
            async (
                Guid id,
                GetCaseHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetCaseQuery(id),
                    cancellationToken);

                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }

                return Results.Ok(result.Value);
            });

        return app;
    }
}