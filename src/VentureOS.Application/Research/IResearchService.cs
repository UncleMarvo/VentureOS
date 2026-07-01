using VentureOS.Application.Research.ResearchCase;
using VentureOS.Domain.Cases;

namespace VentureOS.Application.Research;

public interface IResearchService
{
    Task<ResearchPackageDto> ResearchCaseAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default);
}
