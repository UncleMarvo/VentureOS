using VentureOS.Domain.Common;

namespace VentureOS.Domain.Decisions;

public sealed class Decision : Entity
{
    internal Decision(
        Guid id,
        Guid caseId,
        string question,
        DecisionOutcome outcome,
        string rationale,
        string expectedOutcome,
        Confidence confidence,
        IReadOnlyCollection<Guid> evidenceIds,
        IReadOnlyCollection<Guid> assumptionIds,
        IReadOnlyCollection<Guid> hypothesisIds,
        IReadOnlyCollection<Guid> challengeIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Question = question;
        Outcome = outcome;
        Rationale = rationale;
        ExpectedOutcome = expectedOutcome;
        Confidence = confidence;
        EvidenceIds = evidenceIds;
        AssumptionIds = assumptionIds;
        HypothesisIds = hypothesisIds;
        ChallengeIds = challengeIds;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Question { get; }

    public DecisionOutcome Outcome { get; }

    public string Rationale { get; }

    public string ExpectedOutcome { get; }

    public Confidence Confidence { get; }

    public IReadOnlyCollection<Guid> EvidenceIds { get; }

    public IReadOnlyCollection<Guid> AssumptionIds { get; }

    public IReadOnlyCollection<Guid> HypothesisIds { get; }

    public IReadOnlyCollection<Guid> ChallengeIds { get; }

    public DateTime CreatedAtUtc { get; }

    public static Decision Restore(
        Guid id,
        Guid caseId,
        string question,
        DecisionOutcome outcome,
        string rationale,
        string expectedOutcome,
        Confidence confidence,
        IReadOnlyCollection<Guid> evidenceIds,
        IReadOnlyCollection<Guid> assumptionIds,
        IReadOnlyCollection<Guid> hypothesisIds,
        IReadOnlyCollection<Guid> challengeIds,
        DateTime createdAtUtc)
    {
        return new Decision(
            id,
            caseId,
            question,
            outcome,
            rationale,
            expectedOutcome,
            confidence,
            evidenceIds,
            assumptionIds,
            hypothesisIds,
            challengeIds,
            createdAtUtc);
    }
}