using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Research;
using VentureOS.Application.Research.ResearchCase;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaResearchService : IResearchService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaResearchService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<ResearchPackageDto> ResearchCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Using Ollama model: {_options.Model}");

        var package = new ResearchPackageDto(
            ventureCase.Id,
            ventureCase.Mission,
            [],
            [],
            [],
            [],
            []);

        Console.WriteLine($"{_options.BaseUrl}/api/generate");

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            new
            {
                model = _options.Model,
                prompt = ResearchCasePrompt.Build(ventureCase),
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

        var researchPackage = JsonSerializer.Deserialize<ResearchPackageDto>(
            ollamaResponse?.Response ?? string.Empty,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (researchPackage is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama research response.");
        }

        return new ResearchPackageDto(
            ventureCase.Id,
            ventureCase.Mission,
            researchPackage.Observations,
            researchPackage.Evidence,
            researchPackage.Assumptions,
            researchPackage.Hypotheses,
            researchPackage.Challenges);
    }
    private sealed record OllamaGenerateResponse(string Response);
}


