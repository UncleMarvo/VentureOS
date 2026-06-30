using System.Security.AccessControl;
using VentureOS.Domain.Common;

namespace VentureOS.Domain.Observations;

public sealed class Observation : Entity
{
    internal Observation(
        Guid id,
        Guid caseId,
        string observationText,
        string summary,
        string sourceReference,
        ObservationSource observationSource,
        Confidence confidence,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        ObservationText = observationText;
        Summary = summary;
        SourceReference = sourceReference;
        ObservationSource = observationSource;
        Confidence = confidence;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string ObservationText { get; }

    public string Summary { get; }

    public string SourceReference { get; }

    public ObservationSource ObservationSource { get; }

    public Confidence Confidence { get; }

    public DateTime CreatedAtUtc { get; }
}