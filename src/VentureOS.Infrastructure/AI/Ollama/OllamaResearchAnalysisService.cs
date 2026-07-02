using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchAnalysis;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchAnalysisService : IResearchAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaResearchAnalysisService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<ResearchAnalysisResultDto> AnalyzeAsync(
        Case ventureCase,
        ResearchEvidencePlanDto evidencePlan,
        EvidenceAcquisitionResultDto acquiredEvidence,
        CancellationToken cancellationToken = default)
    {
        var prompt = ResearchAnalysisPrompt.Build(
            ventureCase,
            evidencePlan,
            acquiredEvidence);

        var analysisText = await GenerateAsync(prompt, cancellationToken);

        return new ResearchAnalysisResultDto(analysisText);
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
