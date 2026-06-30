using VentureOS.Domain.Common;

namespace VentureOS.Domain.Assumptions;

public sealed class Assumption : Entity
{
    internal Assumption(
        Guid id,
        Guid caseId,
        string statement,
        string rationale,
        Confidence confidence,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Statement = statement;
        Rationale = rationale;
        Confidence = confidence;
        Status = AssumptionStatus.Proposed;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Statement { get; }

    public string Rationale { get; }

    public Confidence Confidence { get; }

    public AssumptionStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }
}