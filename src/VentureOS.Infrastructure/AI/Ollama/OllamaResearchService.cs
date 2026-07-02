using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Research;
using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchCase;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Application.Research.ResearchQuality;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchService : IResearchService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly IEvidenceAcquisitionService _evidenceAcquisitionService;

    public OllamaResearchService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        IEvidenceAcquisitionService evidenceAcquisitionService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _evidenceAcquisitionService = evidenceAcquisitionService;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<ResearchPackageDto> ResearchCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Using Ollama model: {_options.Model}");

        var evidencePlanPrompt = ResearchEvidencePlanningPrompt.Build(ventureCase);
        var evidencePlanJson = await GenerateAsync(
            evidencePlanPrompt,
            cancellationToken);

        var evidencePlan = JsonSerializer.Deserialize<ResearchEvidencePlanDto>(
            evidencePlanJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Console.WriteLine("========================================");
        Console.WriteLine("EVIDENCE PLAN");
        Console.WriteLine("========================================");
        Console.WriteLine(
            JsonSerializer.Serialize(
                evidencePlan,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

        if (evidencePlan is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama evidence plan response.");
        }

        var acquiredEvidence = await _evidenceAcquisitionService.AcquireEvidenceAsync(
            ventureCase.Id,
            evidencePlan.Questions,
            cancellationToken);

        Console.WriteLine("========================================");
        Console.WriteLine("ACQUIRED EVIDENCE");
        Console.WriteLine("========================================");

        Console.WriteLine(
            JsonSerializer.Serialize(
                acquiredEvidence,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

        var analysisPrompt = ResearchAnalysisPrompt.Build(
            ventureCase,
            evidencePlan,
            acquiredEvidence);

        Console.WriteLine("========================================");
        Console.WriteLine("ANALYSIS PROMPT");
        Console.WriteLine("========================================");
        Console.WriteLine(analysisPrompt);

        var analysisText = await GenerateAsync(
            analysisPrompt,
            cancellationToken);

        Console.WriteLine("========================================");
        Console.WriteLine("ANALYSIS");
        Console.WriteLine("========================================");
        Console.WriteLine(analysisText);

        var extractionPrompt = ResearchExtractionPrompt.Build(analysisText);

        var json = await GenerateAsync(
            extractionPrompt,
            cancellationToken);

        var researchPackage = JsonSerializer.Deserialize<ResearchPackageDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Console.WriteLine("========================================");
        Console.WriteLine("EXTRACTION");
        Console.WriteLine("========================================");
        Console.WriteLine(researchPackage);

        stopwatch.Stop();

        if (researchPackage is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama research response.");
        }

        // QUALITY CHECK
        var qualityIssues = ResearchQualityChecker.Check(researchPackage);

        foreach (var issue in qualityIssues)
        {
            Console.WriteLine(
                $"Research quality issue [{issue.Severity}] {issue.Code} at {issue.Path}: {issue.Message}");
        }

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

        return new ResearchPackageDto(
            ventureCase.Id,
            ventureCase.Mission,
            researchGeneration,
            researchPackage.Observations,
            researchPackage.Evidence,
            researchPackage.Assumptions,
            researchPackage.Opportunities,
            researchPackage.Hypotheses,
            researchPackage.Challenges);
    }

    private async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            new
            {
                model = _options.Model,
                prompt,
                stream = false
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Ollama request failed. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {errorBody}");
        }

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
            cancellationToken);

        if (string.IsNullOrWhiteSpace(ollamaResponse?.Response))
        {
            throw new InvalidOperationException("Ollama returned an empty response.");
        }

        return ollamaResponse.Response;
    }

    private sealed record OllamaGenerateResponse(string Response);
}
