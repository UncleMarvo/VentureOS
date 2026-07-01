using VentureOS.Application.Cases;
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

    public async Task<Result<ResearchPackageDto>> HandleAsync(
        ResearchCaseQuery query,
        CancellationToken cancellationToken = default)
    {
        var ventureCase = await _caseRepository.GetByIdAsync(
            query.CaseId,
            cancellationToken);

        if (ventureCase is null)
        {
            return Result<ResearchPackageDto>.Failure("Case not found.");
        }

        // TODO: Replace with AI research service.
        var researchPackage = await _researchService.ResearchCaseAsync(
            ventureCase,
            cancellationToken);

        return Result<ResearchPackageDto>.Success(researchPackage);
    }
}
