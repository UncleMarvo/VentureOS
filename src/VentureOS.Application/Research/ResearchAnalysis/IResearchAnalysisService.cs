using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Domain.Cases;

namespace VentureOS.Application.Research.ResearchAnalysis;

public interface IResearchAnalysisService
{
    Task<ResearchAnalysisResultDto> AnalyzeAsync(
        Case ventureCase,
        ResearchEvidencePlanDto evidencePlan,
        EvidenceAcquisitionResultDto acquiredEvidence,
        CancellationToken cancellationToken = default);
}
