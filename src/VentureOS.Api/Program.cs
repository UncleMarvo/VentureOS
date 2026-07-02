using VentureOS.Api.Endpoints;
using VentureOS.Infrastructure;
using VentureOS.Infrastructure.AI.Ollama;
using VentureOS.Infrastructure.Persistence.DuckDb;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
app.MapAssumptionEndpoints();
app.MapOpportunityEndpoints();
app.MapHypothesisEndpoints();
app.MapChallengeEndpoints();
app.MapDecisionEndpoints();
app.MapLessonEndpoints();

app.MapResearchEndpoints();
app.MapRedTeamEndpoints();
app.MapBoardEndpoints();

app.Run();


