using VentureOS.Application.Cases;
using VentureOS.Application.Research.ResearchQuality;
using VentureOS.Domain.Common;

namespace VentureOS.Application.Research.ResearchCase;

public sealed class ResearchCaseHandler
{
    private readonly ICaseRepository _caseRepository;
    private readonly IResearchService _researchService;

    public ResearchCaseHandler(
        ICaseRepository caseRepository,
        IResearchService researchService)
    {
        _caseRepository = caseRepository;
        _researchService = researchService;
    }

    public async Task<Result<ResearchCaseResultDto>> HandleAsync(
        ResearchCaseQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<ResearchCaseResultDto>.Failure("Case not found.");
        }

        var researchPackage = await _researchService.ResearchCaseAsync(
            ventureCase,
            cancellationToken);

        var qualityIssues = ResearchQualityChecker.Check(researchPackage);

        return Result<ResearchCaseResultDto>.Success(
            new ResearchCaseResultDto(
                researchPackage,
                qualityIssues));
    }
}
