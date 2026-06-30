using Microsoft.Extensions.DependencyInjection;
using VentureOS.Application.Cases;
using VentureOS.Application.Cases.AddObservation;
using VentureOS.Application.Cases.CreateAssumption;
using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.CreateEvidence;
using VentureOS.Application.Cases.CreateHypothesis;
using VentureOS.Application.Cases.GetCase;
using VentureOS.Application.Cases.RaiseChallenge;
using VentureOS.Application.Cases.RecordDecision;
using VentureOS.Application.Cases.RecordLesson;
using VentureOS.Infrastructure.Persistence.DuckDb;

namespace VentureOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVentureOs(
        this IServiceCollection services)
    {
        services.AddSingleton(new DuckDbOptions());

        services.AddSingleton<DuckDbConnectionFactory>();

        services.AddSingleton<DuckDbSchemaInitializer>();

        services.AddScoped<ICaseRepository, DuckDbCaseRepository>();

        services.AddScoped<CreateCaseHandler>();
        services.AddScoped<GetCaseHandler>();
        services.AddScoped<AddObservationHandler>();
        services.AddScoped<CreateEvidenceHandler>();
        services.AddScoped<CreateAssumptionHandler>();
        services.AddScoped<CreateHypothesisHandler>();
        services.AddScoped<RaiseChallengeHandler>();
        services.AddScoped<RecordDecisionHandler>();
        services.AddScoped<RecordLessonHandler>();

        return services;
    }
}