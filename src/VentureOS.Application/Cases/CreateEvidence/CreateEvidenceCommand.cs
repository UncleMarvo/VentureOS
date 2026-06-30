using VentureOS.Domain.Evidence;

namespace VentureOS.Application.Cases.CreateEvidence;

public sealed record CreateEvidenceCommand(
    Guid CaseId,
    string Summary,
    string Interpretation,
    EvidenceDirection Direction,
    IReadOnlyCollection<Guid> ObservationIds);