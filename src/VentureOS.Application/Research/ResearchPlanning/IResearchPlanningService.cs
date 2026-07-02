using VentureOS.Domain.Cases;

namespace VentureOS.Application.Research.ResearchPlanning;

public interface IResearchPlanningService
{
    Task<ResearchEvidencePlanDto> PlanResearchAsync(
        Case ventureCase,
        CancellationToken cancellationToken = default);
}
