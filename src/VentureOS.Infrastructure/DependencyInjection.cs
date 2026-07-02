using Microsoft.Extensions.DependencyInjection;
using VentureOS.Application.Cases;
using VentureOS.Application.Cases.AddObservation;
using VentureOS.Application.Cases.CreateAssumption;
using VentureOS.Application.Cases.CreateCase;
using VentureOS.Application.Cases.CreateEvidence;
using VentureOS.Application.Cases.CreateHypothesis;
using VentureOS.Application.Cases.CreateOpportunity;
using VentureOS.Application.Cases.GetCase;
using VentureOS.Application.Cases.GetCaseBrief;
using VentureOS.Application.Cases.GetCaseTimeline;
using VentureOS.Application.Cases.RaiseChallenge;
using VentureOS.Application.Cases.RecordDecision;
using VentureOS.Application.Cases.RecordLesson;
using VentureOS.Application.Board;
using VentureOS.Application.Decisions.GetDecisionContext;
using VentureOS.Application.RedTeam;
using VentureOS.Application.RedTeam.AcceptRedTeamReview;
using VentureOS.Application.RedTeam.RedTeamCase;
using VentureOS.Application.Research;
using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchAnalysis;
using VentureOS.Application.Research.ResearchCase;
using VentureOS.Application.Research.ResearchExtraction;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Infrastructure.AI.Ollama;
using VentureOS.Infrastructure.Persistence.DuckDb;
using VentureOS.Infrastructure.Persistence.DuckDb.Stores;

namespace VentureOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVentureOs(
        this IServiceCollection services)
    {
        services.AddSingleton(new DuckDbOptions());

        services.AddSingleton<DuckDbConnectionFactory>();

        services.AddSingleton<DuckDbSchemaInitializer>();

        services.AddScoped<ObservationStore>();
        services.AddScoped<EvidenceStore>();
        services.AddScoped<AssumptionStore>();
        services.AddScoped<OpportunityStore>();
        services.AddScoped<HypothesisStore>();
        services.AddScoped<ChallengeStore>();
        services.AddScoped<DecisionStore>();
        services.AddScoped<LessonStore>();
        services.AddScoped<ICaseRepository, DuckDbCaseRepository>();

        services.AddScoped<CreateCaseHandler>();
        services.AddScoped<GetCaseHandler>();
        services.AddScoped<AddObservationHandler>();
        services.AddScoped<CreateEvidenceHandler>();
        services.AddScoped<CreateAssumptionHandler>();
        services.AddScoped<CreateOpportunityHandler>();
        services.AddScoped<CreateHypothesisHandler>();
        services.AddScoped<RaiseChallengeHandler>();
        services.AddScoped<RecordDecisionHandler>();
        services.AddScoped<RecordLessonHandler>();
        services.AddScoped<GetCaseTimelineHandler>();
        services.AddScoped<GetDecisionContextHandler>();
        services.AddScoped<GetCaseBriefHandler>();
        services.AddScoped<ResearchCaseHandler>();

        services.AddScoped<IResearchService, OllamaResearchService>();
        services
            .AddHttpClient<IEvidenceAcquisitionService, OllamaEvidenceAcquisitionService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        services
            .AddHttpClient<IResearchPlanningService, OllamaResearchPlanningService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        services
            .AddHttpClient<IResearchAnalysisService, OllamaResearchAnalysisService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        services
            .AddHttpClient<IResearchExtractionService, OllamaResearchExtractionService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        services.AddScoped<AcceptResearchPackageHandler>();

        services.AddScoped<RedTeamCaseHandler>();
        services.AddScoped<AcceptRedTeamReviewHandler>();
        services
            .AddHttpClient<IRedTeamReviewService, OllamaRedTeamReviewService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });

        services.AddScoped<BoardCaseHandler>();
        services
            .AddHttpClient<IBoardReviewService, OllamaBoardReviewService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(10);
            });

        return services;
    }
}
