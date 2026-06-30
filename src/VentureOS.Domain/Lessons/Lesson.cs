using VentureOS.Domain.Common;

namespace VentureOS.Domain.Lessons;

public sealed class Lesson : Entity
{
    internal Lesson(
        Guid id,
        Guid caseId,
        string summary,
        string detail,
        Confidence confidence,
        IReadOnlyCollection<Guid> decisionIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Summary = summary;
        Detail = detail;
        Confidence = confidence;
        DecisionIds = decisionIds;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Summary { get; }

    public string Detail { get; }

    public Confidence Confidence { get; }

    public IReadOnlyCollection<Guid> DecisionIds { get; }

    public DateTime CreatedAtUtc { get; }
}