using VentureOS.Domain.Common;

namespace VentureOS.Domain.Observations;

public sealed class Observation : Entity
{
    internal Observation(
        Guid id,
        Guid caseId,
        string summary,
        string source,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Summary = summary;
        Source = source;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Summary { get; }

    public string Source { get; }

    public DateTime CreatedAtUtc { get; }
}