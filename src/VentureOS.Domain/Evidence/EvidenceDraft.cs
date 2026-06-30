namespace VentureOS.Domain.Evidence;

public sealed record EvidenceDraft(
    string Summary,
    string Interpretation,
    EvidenceDirection Direction,
    IReadOnlyCollection<Guid> ObservationIds);