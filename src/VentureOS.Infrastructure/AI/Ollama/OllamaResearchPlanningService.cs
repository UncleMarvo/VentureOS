using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchPlanningService : IResearchPlanningService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaResearchPlanningService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<ResearchEvidencePlanDto> PlanResearchAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        var prompt = ResearchEvidencePlanningPrompt.Build(ventureCase);

        var json = await GenerateAsync(prompt, cancellationToken);

        var evidencePlan = JsonSerializer.Deserialize<ResearchEvidencePlanDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (evidencePlan is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama evidence plan response.");
        }

        return evidencePlan;
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
