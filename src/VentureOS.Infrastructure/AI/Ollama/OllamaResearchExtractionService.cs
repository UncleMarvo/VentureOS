using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Research.ResearchExtraction;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchExtractionService : IResearchExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaResearchExtractionService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<ExtractedResearchDto> ExtractAsync(
        string researchAnalysis,
        CancellationToken cancellationToken = default)
    {
        var prompt = ResearchExtractionPrompt.Build(researchAnalysis);

        var json = await GenerateAsync(prompt, cancellationToken);

        var extracted = JsonSerializer.Deserialize<ExtractedResearchDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (extracted is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama research extraction response.");
        }

        return extracted;
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
