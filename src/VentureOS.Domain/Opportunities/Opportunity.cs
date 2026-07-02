using VentureOS.Domain.Common;

namespace VentureOS.Domain.Opportunities;

public sealed class Opportunity : Entity
{
    internal Opportunity(
        Guid id,
        Guid caseId,
        string statement,
        string customerValue,
        string commercialValue,
        string differentiation,
        string timing,
        Confidence confidence,
        IReadOnlyCollection<Guid> evidenceIds,
        IReadOnlyCollection<Guid> assumptionIds,
        DateTime createdAtUtc) : base(id)
    {
        CaseId = caseId;
        Statement = statement;
        CustomerValue = customerValue;
        CommercialValue = commercialValue;
        Differentiation = differentiation;
        Timing = timing;
        Confidence = confidence;
        EvidenceIds = evidenceIds;
        AssumptionIds = assumptionIds;
        Status = OpportunityStatus.Proposed;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid CaseId { get; }

    public string Statement { get; }

    public string CustomerValue { get; }

    public string CommercialValue { get; }

    public string Differentiation { get; }

    public string Timing { get; }

    public Confidence Confidence { get; }

    public IReadOnlyCollection<Guid> EvidenceIds { get; }

    public IReadOnlyCollection<Guid> AssumptionIds { get; }

    public OpportunityStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Opportunity Restore(
        Guid id,
        Guid caseId,
        string statement,
        string customerValue,
        string commercialValue,
        string differentiation,
        string timing,
        Confidence confidence,
        IReadOnlyCollection<Guid> evidenceIds,
        IReadOnlyCollection<Guid> assumptionIds,
        OpportunityStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        var opportunity = new Opportunity(
            id,
            caseId,
            statement,
            customerValue,
            commercialValue,
            differentiation,
            timing,
            confidence,
            evidenceIds,
            assumptionIds,
            createdAtUtc);

        opportunity.Status = status;
        opportunity.UpdatedAtUtc = updatedAtUtc;

        return opportunity;
    }

    public Result MarkSupported()
    {
        if (Status is OpportunityStatus.Rejected or OpportunityStatus.Superseded)
        {
            return Result.Failure("Rejected or superseded opportunities cannot be marked as supported.");
        }

        Status = OpportunityStatus.Supported;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkChallenged()
    {
        if (Status is OpportunityStatus.Rejected or OpportunityStatus.Superseded)
        {
            return Result.Failure("Rejected or superseded opportunities cannot be challenged.");
        }

        Status = OpportunityStatus.Challenged;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Accept()
    {
        if (Status == OpportunityStatus.Rejected)
        {
            return Result.Failure("Rejected opportunities cannot be accepted.");
        }

        if (Status == OpportunityStatus.Superseded)
        {
            return Result.Failure("Superseded opportunities cannot be accepted.");
        }

        Status = OpportunityStatus.Accepted;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Reject()
    {
        if (Status == OpportunityStatus.Accepted)
        {
            return Result.Failure("Accepted opportunities cannot be rejected.");
        }

        if (Status == OpportunityStatus.Superseded)
        {
            return Result.Failure("Superseded opportunities cannot be rejected.");
        }

        Status = OpportunityStatus.Rejected;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Supersede()
    {
        if (Status == OpportunityStatus.Accepted)
        {
            return Result.Failure("Accepted opportunities cannot be superseded.");
        }

        if (Status == OpportunityStatus.Rejected)
        {
            return Result.Failure("Rejected opportunities cannot be superseded.");
        }

        Status = OpportunityStatus.Superseded;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}
