namespace VentureOS.Application.Research.ResearchExtraction;

public interface IResearchExtractionService
{
    Task<ExtractedResearchDto> ExtractAsync(
        string researchAnalysis,
        CancellationToken cancellationToken = default);
}
