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
        IReadOnlyCollection<Guid> assumptionIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Statement = statement;
        Reasoning = reasoning;
        ExpectedOutcome = expectedOutcome;
        SuccessCriteria = successCriteria;
        Confidence = confidence;
        EvidenceIds = evidenceIds;
        AssumptionIds = assumptionIds;
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

    public IReadOnlyCollection<Guid> AssumptionIds { get; }

    public HypothesisStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public Result MarkSupported()
    {
        if (Status is HypothesisStatus.Rejected or HypothesisStatus.Superseded)
        {
            return Result.Failure("Rejected or superseded hypotheses cannot be marked as supported.");
        }

        Status = HypothesisStatus.Supported;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkChallenged()
    {
        if (Status is HypothesisStatus.Rejected or HypothesisStatus.Superseded)
        {
            return Result.Failure("Rejected or superseded hypotheses cannot be challenged.");
        }

        Status = HypothesisStatus.Challenged;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Accept()
    {
        if (Status == HypothesisStatus.Rejected)
        {
            return Result.Failure("Rejected hypotheses cannot be accepted.");
        }

        if (Status == HypothesisStatus.Superseded)
        {
            return Result.Failure("Superseded hypotheses cannot be accepted.");
        }

        Status = HypothesisStatus.Accepted;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Reject()
    {
        if (Status == HypothesisStatus.Accepted)
        {
            return Result.Failure("Accepted hypotheses cannot be rejected.");
        }

        if (Status == HypothesisStatus.Superseded)
        {
            return Result.Failure("Superseded hypotheses cannot be rejected.");
        }

        Status = HypothesisStatus.Rejected;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Supersede()
    {
        if (Status == HypothesisStatus.Accepted)
        {
            return Result.Failure("Accepted hypotheses cannot be superseded.");
        }

        if (Status == HypothesisStatus.Rejected)
        {
            return Result.Failure("Rejected hypotheses cannot be superseded.");
        }

        Status = HypothesisStatus.Superseded;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}