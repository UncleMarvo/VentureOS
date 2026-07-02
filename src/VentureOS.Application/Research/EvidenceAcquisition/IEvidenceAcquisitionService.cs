using VentureOS.Application.Research.ResearchPlanning;

namespace VentureOS.Application.Research.EvidenceAcquisition;

public interface IEvidenceAcquisitionService
{
    Task<EvidenceAcquisitionResultDto> AcquireEvidenceAsync(
        Guid caseId,
        IReadOnlyList<ResearchQuestionDto> researchQuestions,
        CancellationToken cancellationToken = default);
}
