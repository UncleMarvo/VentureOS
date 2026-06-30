using VentureOS.Domain.Common;

namespace VentureOS.Domain.Evidence;

public sealed class Evidence : Entity
{
    internal Evidence(
        Guid id,
        Guid caseId,
        string summary,
        string interpretation,
        EvidenceDirection direction,
        IReadOnlyCollection<Guid> observationIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Summary = summary;
        Interpretation = interpretation;
        Direction = direction;
        ObservationIds = observationIds;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Summary { get; }

    public string Interpretation { get; }

    public EvidenceDirection Direction { get; }

    public IReadOnlyCollection<Guid> ObservationIds { get; }

    public DateTime CreatedAtUtc { get; }
}