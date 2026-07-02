using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using VentureOS.Application.Cases;
using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Infrastructure.AI.Prompts;

namespace VentureOS.Infrastructure.AI.Ollama;

public sealed class OllamaEvidenceAcquisitionService : IEvidenceAcquisitionService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ICaseRepository _caseRepository;

    public OllamaEvidenceAcquisitionService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ICaseRepository caseRepository)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _caseRepository = caseRepository;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<EvidenceAcquisitionResultDto> AcquireEvidenceAsync(
        Guid caseId,
        IReadOnlyList<ResearchQuestionDto> researchQuestions,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            caseId,
            cancellationToken);

        if (ventureCase is null)
        {
            throw new InvalidOperationException("Case not found.");
        }

        var selectedQuestions = researchQuestions
            .OrderBy(question => question.Priority)
            .Take(3)
            .ToList();

        var acquiredEvidence = new List<AcquiredEvidenceDto>();

        foreach (var question in selectedQuestions)
        {
            var prompt = EvidenceAcquisitionPrompt.Build(
                ventureCase,
                question);

            var json = await GenerateAsync(
                prompt,
                cancellationToken);

            var evidence = JsonSerializer.Deserialize<AcquiredEvidenceDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (evidence is null)
            {
                throw new InvalidOperationException("Failed to deserialize Ollama evidence acquisition response.");
            }

            acquiredEvidence.Add(evidence);
        }

        return new EvidenceAcquisitionResultDto(acquiredEvidence);
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
            throw new InvalidOperationException("Ollama returned an empty evidence acquisition response.");
        }

        return ollamaResponse.Response;
    }

    private sealed record OllamaGenerateResponse(string Response);
}
