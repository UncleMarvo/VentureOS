using VentureOS.Domain.Common;

namespace VentureOS.Domain.Challenges;

public sealed class Challenge : Entity
{
    internal Challenge(
        Guid id,
        Guid caseId,
        ChallengeTarget target,
        Guid targetId,
        string statement,
        string reasoning,
        Confidence confidence,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Target = target;
        TargetId = targetId;
        Statement = statement;
        Reasoning = reasoning;
        Confidence = confidence;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public ChallengeTarget Target { get; }

    public Guid TargetId { get; }

    public string Statement { get; }

    public string Reasoning { get; }

    public Confidence Confidence { get; }

    public DateTime CreatedAtUtc { get; }

    public static Challenge Restore(
        Guid id,
        Guid caseId,
        ChallengeTarget target,
        Guid targetId,
        string statement,
        string reasoning,
        Confidence confidence,
        DateTime createdAtUtc)
    {
        return new Challenge(
            id,
            caseId,
            target,
            targetId,
            statement,
            reasoning,
            confidence,
            createdAtUtc);
    }
}