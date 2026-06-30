using VentureOS.Api.Endpoints;
using VentureOS.Application.Cases.AddObservation;
using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.CreateEvidence;
using VentureOS.Application.Cases.GetCase;
using VentureOS.Domain.Common;
using VentureOS.Domain.Evidence;
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

app.MapCaseEndpoints();
app.MapEvidenceEndpoints();
app.MapObservationEndpoints();

app.Run();


