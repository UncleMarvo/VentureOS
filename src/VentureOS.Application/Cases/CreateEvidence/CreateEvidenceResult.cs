using VentureOS.Domain.Evidence;

namespace VentureOS.Application.Cases.CreateEvidence;

public sealed record CreateEvidenceResult(
    Guid CaseId,
    Guid EvidenceId,
    string Summary,
    EvidenceDirection Direction);