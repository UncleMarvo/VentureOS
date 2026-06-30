using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.GetCase;
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

app.Run();