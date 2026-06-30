using VentureOS.Domain.Common;

namespace VentureOS.Domain.Hypotheses;

public sealed class Hypothesis : Entity
{
    internal Hypothesis(
        Guid id,
        Guid caseId,
        string statement,
        string reasoning,
        string expectedOutcome,
        string successCriteria,
        Confidence confidence,
        IReadOnlyCollection<Guid> evidenceIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Statement = statement;
        Reasoning = reasoning;
        ExpectedOutcome = expectedOutcome;
        SuccessCriteria = successCriteria;
        Confidence = confidence;
        EvidenceIds = evidenceIds;
        Status = HypothesisStatus.Proposed;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Statement { get; }

    public string Reasoning { get; }

    public string ExpectedOutcome { get; }

    public string SuccessCriteria { get; }

    public Confidence Confidence { get; }

    public IReadOnlyCollection<Guid> EvidenceIds { get; }

    public HypothesisStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }
}