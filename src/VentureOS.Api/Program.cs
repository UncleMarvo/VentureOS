using VentureOS.Application.Cases.AddObservation;
using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.GetCase;
using VentureOS.Domain.Common;
using VentureOS.Domain.Observations;
using VentureOS.Infrastructure;
using VentureOS.Infrastructure.Persistence.DuckDb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVentureOs();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<DuckDbSchemaInitializer>();

    await initializer.InitializeAsync();
}

// ==========================================
// CASES
// ==========================================

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

// ==========================================
// OBSERVATIONS
// ==========================================

app.MapPost(
    "/cases/{id:guid}/observations",
    async (
        Guid id,
        AddObservationRequest request,
        AddObservationHandler handler,
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

app.Run();

public sealed record AddObservationRequest(
    string ObservationText,
    string Summary,
    string SourceReference,
    ObservationSource ObservationSource,
    int Confidence);