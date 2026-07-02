using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Board;
using VentureOS.Domain.Decisions;
using VentureOS.Infrastructure.AI.Personas;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaBoardReviewService : IBoardReviewService
{
    private static readonly DecisionOutcome[] ExpectedDecisionOutcomes =
    [
        DecisionOutcome.Approved,
        DecisionOutcome.Rejected,
        DecisionOutcome.Deferred,
        DecisionOutcome.MoreResearchRequired
    ];

    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaBoardReviewService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<BoardNarrativeDto> ReviewAsync(
        BoardDossierDto dossier,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var prompt = BoardReviewPrompt.Build(dossier);

        var json = await GenerateAsync(prompt, cancellationToken);

        var reviewJson = JsonSerializer.Deserialize<BoardReviewJsonDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (reviewJson is null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama board review response.");
        }

        var decisionOptions = ParseDecisionOptions(reviewJson.DecisionOptions);

        stopwatch.Stop();

        var generation = new BoardGenerationDto(
            "Ollama",
            _options.Model,
            BoardPersona.Name,
            BoardPersona.Version,
            BoardReviewPrompt.Version,
            DateTime.UtcNow,
            stopwatch.Elapsed,
            "AI-generated board briefing narrative requiring human review.");

        return new BoardNarrativeDto(
            reviewJson.ExecutiveSummary,
            reviewJson.DecisionFraming,
            reviewJson.Risks,
            reviewJson.OverallConfidenceNarrative,
            reviewJson.RecommendedInvestigations,
            decisionOptions,
            generation);
    }

    private static IReadOnlyList<BoardDecisionOptionDto> ParseDecisionOptions(
        IReadOnlyList<BoardDecisionOptionJsonDto> options)
    {
        if (options.Count != ExpectedDecisionOutcomes.Length)
        {
            throw new InvalidOperationException(
                $"Ollama board review response returned {options.Count} decision options, expected {ExpectedDecisionOutcomes.Length}.");
        }

        var parsed = new List<BoardDecisionOptionDto>(options.Count);

        for (var i = 0; i < options.Count; i++)
        {
            if (!Enum.TryParse<DecisionOutcome>(options[i].Outcome, out var outcome) ||
                outcome != ExpectedDecisionOutcomes[i])
            {
                throw new InvalidOperationException(
                    $"Ollama board review response returned decision option '{options[i].Outcome}' at position {i}, expected '{ExpectedDecisionOutcomes[i]}'.");
            }

            parsed.Add(new BoardDecisionOptionDto(outcome, options[i].Rationale));
        }

        return parsed;
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
            throw new InvalidOperationException("Ollama returned an empty board review response.");
        }

        return ollamaResponse.Response;
    }

    private sealed record OllamaGenerateResponse(string Response);

    private sealed record BoardReviewJsonDto(
        string ExecutiveSummary,
        string DecisionFraming,
        IReadOnlyList<string> Risks,
        string OverallConfidenceNarrative,
        IReadOnlyList<string> RecommendedInvestigations,
        IReadOnlyList<BoardDecisionOptionJsonDto> DecisionOptions);

    private sealed record BoardDecisionOptionJsonDto(
        string Outcome,
        string Rationale);
}
