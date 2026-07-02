using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.RedTeam;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaRedTeamReviewService : IRedTeamReviewService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaRedTeamReviewService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<RedTeamReviewResultDto> ReviewCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var prompt = RedTeamReviewPrompt.Build(ventureCase);

        var json = await GenerateAsync(prompt, cancellationToken);

        var reviewJson = JsonSerializer.Deserialize<RedTeamReviewJsonDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (reviewJson is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama red team review response.");
        }

        stopwatch.Stop();

        var generation = new RedTeamGenerationDto(
            "Ollama",
            _options.Model,
            RedTeamAnalystPersona.Name,
            RedTeamAnalystPersona.Version,
            RedTeamReviewPrompt.Version,
            DateTime.UtcNow,
            stopwatch.Elapsed,
            "AI-generated red team review requiring human review.");

        return new RedTeamReviewResultDto(
            ventureCase.Id,
            ventureCase.Mission,
            generation,
            reviewJson.Challenges);
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
            throw new InvalidOperationException("Ollama returned an empty red team review response.");
        }

        return ollamaResponse.Response;
    }

    private sealed record OllamaGenerateResponse(string Response);

    private sealed record RedTeamReviewJsonDto(IReadOnlyList<RedTeamProposedChallengeDto> Challenges);
}
