using Microsoft.Extensions.Options;
using System.Diagnostics;
using VentureOS.Application.Research;
using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchAnalysis;
using VentureOS.Application.Research.ResearchCase;
using VentureOS.Application.Research.ResearchExtraction;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Application.Research.ResearchQuality;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchService : IResearchService
{
    private readonly OllamaOptions _options;
    private readonly IResearchPlanningService _researchPlanningService;
    private readonly IEvidenceAcquisitionService _evidenceAcquisitionService;
    private readonly IResearchAnalysisService _researchAnalysisService;
    private readonly IResearchExtractionService _researchExtractionService;

    public OllamaResearchService(
        IOptions<OllamaOptions> options,
        IResearchPlanningService researchPlanningService,
        IEvidenceAcquisitionService evidenceAcquisitionService,
        IResearchAnalysisService researchAnalysisService,
        IResearchExtractionService researchExtractionService)
    {
        _options = options.Value;
        _researchPlanningService = researchPlanningService;
        _evidenceAcquisitionService = evidenceAcquisitionService;
        _researchAnalysisService = researchAnalysisService;
        _researchExtractionService = researchExtractionService;
    }

    public async Task<ResearchPackageDto> ResearchCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var evidencePlan = await _researchPlanningService.PlanResearchAsync(
            ventureCase,
            cancellationToken);

        var acquiredEvidence = await _evidenceAcquisitionService.AcquireEvidenceAsync(
            ventureCase.Id,
            evidencePlan.Questions,
            cancellationToken);

        var analysisResult = await _researchAnalysisService.AnalyzeAsync(
            ventureCase,
            evidencePlan,
            acquiredEvidence,
            cancellationToken);

        var extracted = await _researchExtractionService.ExtractAsync(
            analysisResult.AnalysisText,
            cancellationToken);

        stopwatch.Stop();

        var promptVersion =
            $"{ResearchEvidencePlanningPrompt.Version}+{EvidenceAcquisitionPrompt.Version}" +
            $"+{ResearchAnalysisPrompt.Version}+{ResearchExtractionPrompt.Version}";

        var researchGeneration = new ResearchGenerationDto(
            "Ollama",
            _options.Model,
            ResearchAnalystPersona.Name,
            ResearchAnalystPersona.Version,
            promptVersion,
            DateTime.UtcNow,
            stopwatch.Elapsed,
            "AI-generated research requiring human review.");

        var researchPackage = new ResearchPackageDto(
            ventureCase.Id,
            ventureCase.Mission,
            researchGeneration,
            extracted.Observations,
            extracted.Evidence,
            extracted.Assumptions,
            extracted.Opportunities,
            extracted.Hypotheses,
            extracted.Challenges);

        var qualityIssues = ResearchQualityChecker.Check(researchPackage);

        foreach (var issue in qualityIssues)
        {
            Console.WriteLine(
                $"Research quality issue [{issue.Severity}] {issue.Code} at {issue.Path}: {issue.Message}");
        }

        return researchPackage;
    }
}
